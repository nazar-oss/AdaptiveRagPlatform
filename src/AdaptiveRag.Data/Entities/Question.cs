namespace AdaptiveRag.Data.Entities;

public class Question
{
    public Guid Id { get; set; }
    public Guid TestId { get; set; }
    public Test Test { get; set; } = null!;

    public Guid? SourceChunkId { get; set; }
    public MaterialChunk? SourceChunk { get; set; }

    public string QuestionText { get; set; } = string.Empty;
    public string QuestionType { get; set; } = "SingleChoice"; // SingleChoice | OpenEnded
    public string? OptionsJson { get; set; }
    public string CorrectAnswer { get; set; } = string.Empty;
    public int DifficultyLevel { get; set; } = 3; // 1..5
    public bool IsAiGenerated { get; set; }

    public ICollection<StudentAnswer> StudentAnswers { get; set; } = [];
}
