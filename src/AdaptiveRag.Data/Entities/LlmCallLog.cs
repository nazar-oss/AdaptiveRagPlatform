namespace AdaptiveRag.Data.Entities;

public class LlmCallLog
{
    public Guid Id { get; set; }
    public Guid? AttemptId { get; set; }
    public TestAttempt? Attempt { get; set; }

    public Guid? MaterialId { get; set; }
    public CourseMaterial? Material { get; set; }

    public string OperationType { get; set; } = string.Empty; // Embedding | QuestionGen | AnswerEval
    public string ModelName { get; set; } = string.Empty;
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public int LatencyMs { get; set; }
    public bool IsSuccess { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
