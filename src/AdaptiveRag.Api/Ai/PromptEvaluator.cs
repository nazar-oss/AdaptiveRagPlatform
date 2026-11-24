using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace AdaptiveRag.Api.Ai;

public class TestCase
{
    public int Id { get; set; }
    public string Text { get; set; } = "";
    public string ExpectedCategory { get; set; } = "";
    public int? ExpectedMaterialId { get; set; }
    public bool ExpectTool { get; set; }
}

public class PromptEvaluator(StudyAssistant ai, IOptions<AiOptions> options)
{
    public async Task RunAsync()
    {
        var json = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Tests", "test-cases.json"));
        var cases = JsonSerializer.Deserialize<List<TestCase>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        var table = new StringBuilder();
        table.AppendLine("| version | valid JSON | category | material_id | tool | tokens | ms |");
        table.AppendLine("|---|---|---|---|---|---|---|");

        foreach (var v in new[] { "v1", "v2", "v3" })
        {
            int valid = 0, cat = 0, mid = 0, tool = 0;
            long tokens = 0, ms = 0;

            foreach (var c in cases)
            {
                var sw = Stopwatch.StartNew();
                var r = await ai.HandleAsync(c.Text, v);
                ms += sw.ElapsedMilliseconds;
                tokens += r.InputTokens + r.OutputTokens;

                if (r.Attempts == 1 && r.Analysis != null) valid++;
                if (r.Analysis?.Category == c.ExpectedCategory) cat++;
                else Console.WriteLine($"{v} #{c.Id}: expected {c.ExpectedCategory}, got {r.Analysis?.Category}");

                if (r.Analysis?.MaterialId == c.ExpectedMaterialId) mid++;
                if ((r.ToolCalls.Count > 0) == c.ExpectTool) tool++;

                await Task.Delay(options.Value.DelayMs);
            }

            int n = cases.Count;
            table.AppendLine($"| {v} | {valid}/{n} | {cat}/{n} | {mid}/{n} | {tool}/{n} | {tokens / n} | {ms / n} |");
        }

        Console.WriteLine(table);
        File.WriteAllText("results.md", table.ToString());
    }
}