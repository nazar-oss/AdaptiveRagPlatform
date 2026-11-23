namespace AdaptiveRag.Data.Entities;

public class TestAttempt
{
    public Guid Id { get; set; }
    public Guid TestId { get; set; }
    public Test Test { get; set; } = null!;

    public Guid StudentId { get; set; }
    public User Student { get; set; } = null!;

    public int CurrentDifficulty { get; set; } = 3; // 1..5
    public double FinalScore { get; set; }
    public string Status { get; set; } = "InProgress"; // InProgress | Completed
    public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAtUtc { get; set; }

    public ICollection<StudentAnswer> Answers { get; set; } = [];
    public ICollection<LlmCallLog> LlmCallLogs { get; set; } = [];
}
