namespace AdaptiveRag.Data.Entities;

public class Test
{
    public Guid Id { get; set; }
    public Guid MaterialId { get; set; }
    public CourseMaterial Material { get; set; } = null!;

    public Guid CreatorId { get; set; }
    public User Creator { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public int InitialDifficulty { get; set; } = 3; // 1..5
    public int MaxQuestions { get; set; } = 10;
    public bool IsPublished { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<Question> Questions { get; set; } = [];
    public ICollection<TestAttempt> Attempts { get; set; } = [];
}
