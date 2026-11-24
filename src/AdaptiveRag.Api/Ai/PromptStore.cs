using System.Text.Json;
using Microsoft.Extensions.AI;

namespace AdaptiveRag.Api.Ai;

public static class PromptStore
{
    private static string Dir(string version) =>
        Path.Combine(Directory.GetCurrentDirectory(), "Prompts", version);

    public static string Load(string version, string file) =>
        File.ReadAllText(Path.Combine(Dir(version), file));

    public static List<ChatMessage> LoadFewShot(string version)
    {
        var list = new List<ChatMessage>();
        var path = Path.Combine(Dir(version), "fewshot.json");
        if (!File.Exists(path)) return list;

        var doc = JsonDocument.Parse(File.ReadAllText(path));
        foreach (var ex in doc.RootElement.EnumerateArray())
        {
            list.Add(new ChatMessage(ChatRole.User, ex.GetProperty("user").GetString()!));
            list.Add(new ChatMessage(ChatRole.Assistant, ex.GetProperty("assistant").GetRawText()));
        }
        return list;
    }
}