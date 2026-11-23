namespace AdaptiveRag.Data.Entities;

public class CourseMaterial
{
    public Guid Id { get; set; }
    public Guid AuthorId { get; set; }
    public User Author { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string SubjectArea { get; set; } = string.Empty;
    public string RawContent { get; set; } = string.Empty;
    public string IndexingStatus { get; set; } = "Pending"; // Pending | Indexed | Failed
    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<MaterialChunk> Chunks { get; set; } = [];
    public ICollection<Test> Tests { get; set; } = [];
    public ICollection<LlmCallLog> LlmCallLogs { get; set; } = [];
}
