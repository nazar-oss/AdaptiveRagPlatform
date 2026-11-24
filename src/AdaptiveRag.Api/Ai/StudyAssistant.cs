using System.ClientModel;
using System.Text.Json;
using AdaptiveRag.Data;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OpenAI;
namespace AdaptiveRag.Api.Ai;

public record AssistantResult(
    RequestAnalysis? Analysis,
    int Attempts,
    string Answer,
    List<string> ToolCalls,
    long InputTokens,
    long OutputTokens);

public class StudyAssistant
{
    private readonly OpenAI.Chat.ChatClient _chat;
    private readonly IChatClient _client;
    private readonly AdaptiveRagDbContext _db;
    private readonly string _defaultVersion;

    public StudyAssistant(AdaptiveRagDbContext db, IOptions<AiOptions> options)
    {
        var o = options.Value;
        var openAi = new OpenAIClient(
            new ApiKeyCredential(o.ApiKey),
            new OpenAIClientOptions { Endpoint = new Uri("https://api.cohere.ai/compatibility/v1") });

        _chat = openAi.GetChatClient(o.Model);
        _client = _chat.AsIChatClient();
        _db = db;
        _defaultVersion = o.PromptVersion;
    }

    private async Task<(RequestAnalysis?, int, long, long)> AnalyzeAsync(string text, string version)
    {
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, PromptStore.Load(version, "analysis.md"))
        };
        messages.AddRange(PromptStore.LoadFewShot(version));
        messages.Add(new(ChatRole.User, text));

        var opts = new ChatOptions
        {
            Temperature = 0f,
            ResponseFormat = ChatResponseFormat.Json
        };

        long tin = 0, tout = 0;

        for (int attempt = 1; attempt <= 2; attempt++)
        {
            var resp = await _client.GetResponseAsync(messages, opts);
            tin += resp.Usage?.InputTokenCount ?? 0;
            tout += resp.Usage?.OutputTokenCount ?? 0;

            var errors = new List<string>();
            RequestAnalysis? analysis = null;
            int start = resp.Text.IndexOf('{');
            int end = resp.Text.LastIndexOf('}');

            if (start < 0 || end < start)
            {
                errors.Add("no JSON object found");
            }
            else
            {
                try
                {
                    analysis = JsonSerializer.Deserialize<RequestAnalysis>(resp.Text[start..(end + 1)]);
                    errors = analysis?.Validate() ?? ["empty JSON"];
                }
                catch (JsonException ex)
                {
                    errors.Add("invalid JSON: " + ex.Message);
                }
            }

            if (errors.Count == 0)
                return (analysis, attempt, tin, tout);

            messages.Add(new(ChatRole.Assistant, resp.Text));
            messages.Add(new(ChatRole.User,
                "Invalid JSON response: " + string.Join("; ", errors) + ". Return ONLY the corrected JSON."));
        }

        return (null, 2, tin, tout);
    }

    private async Task<(string, List<string>)> AnswerAsync(string text, string version)
    {
        var tools = new PlatformTools(_db);
        AIAgent agent = _client.AsAIAgent(
            instructions: PromptStore.Load(version, "answer.md"),
            name: "AdaptiveRagAssistant",
            tools: [AIFunctionFactory.Create(tools.GetMaterialInfo, name: "get_material_info")]);

        var run = await agent.RunAsync(text, options: new ChatClientAgentRunOptions(
            new ChatOptions { Temperature = 0f }));

        return (run.Text, tools.Calls);
    }

    public async Task<AssistantResult> HandleAsync(string text, string? version = null)
    {
        version ??= _defaultVersion;
        var (analysis, attempts, tin, tout) = await AnalyzeAsync(text, version);
        var (answer, calls) = await AnswerAsync(text, version);
        return new AssistantResult(analysis, attempts, answer, calls, tin, tout);
    }
}