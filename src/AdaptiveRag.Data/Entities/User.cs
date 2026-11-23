namespace AdaptiveRag.Data.Entities;

public class User
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Student"; // Student | Teacher | Admin
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<CourseMaterial> AuthoredMaterials { get; set; } = [];
    public ICollection<Test> CreatedTests { get; set; } = [];
    public ICollection<TestAttempt> TestAttempts { get; set; } = [];
}
