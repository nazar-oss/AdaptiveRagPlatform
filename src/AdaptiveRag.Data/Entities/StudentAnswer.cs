namespace AdaptiveRag.Data.Entities;

public class StudentAnswer
{
    public Guid Id { get; set; }
    public Guid AttemptId { get; set; }
    public TestAttempt Attempt { get; set; } = null!;

    public Guid QuestionId { get; set; }
    public Question Question { get; set; } = null!;

    public string UserAnswerText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public double ScoreAwarded { get; set; }
    public string? AiFeedback { get; set; }
    public DateTime AnsweredAtUtc { get; set; } = DateTime.UtcNow;
}
