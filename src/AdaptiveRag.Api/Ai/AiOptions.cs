namespace AdaptiveRag.Api.Ai;

public class AiOptions
{
    public string ApiKey { get; set; } = "";
    public string Model { get; set; } = "command-a-03-2025";
    public string PromptVersion { get; set; } = "v2";
    public int DelayMs { get; set; } = 10000;
}