using System.Text.Json.Serialization;

namespace AdaptiveRag.Api.Ai;

public class RequestAnalysis
{
    [JsonPropertyName("category")] public string Category { get; set; } = "";
    [JsonPropertyName("priority")] public string Priority { get; set; } = "";
    [JsonPropertyName("material_id")] public int? MaterialId { get; set; }
    [JsonPropertyName("summary")] public string Summary { get; set; } = "";

    private static readonly string[] ValidCategories =
        ["material_info", "test_help", "technical_issue", "grading_appeal", "other"];

    private static readonly string[] ValidPriorities =
        ["low", "medium", "high"];

    public List<string> Validate()
    {
        var errors = new List<string>();
        if (!ValidCategories.Contains(Category))
            errors.Add($"invalid category '{Category}'");
        if (!ValidPriorities.Contains(Priority))
            errors.Add($"invalid priority '{Priority}'");
        if (string.IsNullOrWhiteSpace(Summary))
            errors.Add("empty summary");
        return errors;
    }
}