namespace AdaptiveRag.Data.Entities;

public class MaterialChunk
{
    public Guid Id { get; set; }
    public Guid MaterialId { get; set; }
    public CourseMaterial Material { get; set; } = null!;

    public int ChunkIndex { get; set; }
    public string ContentText { get; set; } = string.Empty;
    public string QdrantPointId { get; set; } = string.Empty;
    public int TokenCount { get; set; }

    public ICollection<Question> GeneratedQuestions { get; set; } = [];
}
