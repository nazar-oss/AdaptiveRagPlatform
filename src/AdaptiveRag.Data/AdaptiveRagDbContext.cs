using AdaptiveRag.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdaptiveRag.Data;

public class AdaptiveRagDbContext(DbContextOptions<AdaptiveRagDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<CourseMaterial> CourseMaterials => Set<CourseMaterial>();
    public DbSet<MaterialChunk> MaterialChunks => Set<MaterialChunk>();
    public DbSet<Test> Tests => Set<Test>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<TestAttempt> TestAttempts => Set<TestAttempt>();
    public DbSet<StudentAnswer> StudentAnswers => Set<StudentAnswer>();
    public DbSet<LlmCallLog> LlmCallLogs => Set<LlmCallLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.FullName).HasMaxLength(150).IsRequired();
            entity.Property(u => u.Email).HasMaxLength(200).IsRequired();
            entity.Property(u => u.Role).HasMaxLength(30).IsRequired();
        });

        modelBuilder.Entity<CourseMaterial>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Title).HasMaxLength(250).IsRequired();
            entity.Property(m => m.SubjectArea).HasMaxLength(120).IsRequired();
            entity.Property(m => m.IndexingStatus).HasMaxLength(30).IsRequired();

            entity.HasOne(m => m.Author)
                  .WithMany(u => u.AuthoredMaterials)
                  .HasForeignKey(m => m.AuthorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MaterialChunk>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.HasIndex(c => c.QdrantPointId).IsUnique();
            entity.Property(c => c.QdrantPointId).HasMaxLength(100).IsRequired();

            entity.HasOne(c => c.Material)
                  .WithMany(m => m.Chunks)
                  .HasForeignKey(c => c.MaterialId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Test>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Title).HasMaxLength(250).IsRequired();

            entity.HasOne(t => t.Material)
                  .WithMany(m => m.Tests)
                  .HasForeignKey(t => t.MaterialId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(t => t.Creator)
                  .WithMany(u => u.CreatedTests)
                  .HasForeignKey(t => t.CreatorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(q => q.Id);
            entity.Property(q => q.QuestionType).HasMaxLength(40).IsRequired();
            entity.Property(q => q.OptionsJson).HasColumnType("jsonb");

            entity.HasOne(q => q.Test)
                  .WithMany(t => t.Questions)
                  .HasForeignKey(q => q.TestId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(q => q.SourceChunk)
                  .WithMany(c => c.GeneratedQuestions)
                  .HasForeignKey(q => q.SourceChunkId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<TestAttempt>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Status).HasMaxLength(30).IsRequired();

            entity.HasOne(a => a.Test)
                  .WithMany(t => t.Attempts)
                  .HasForeignKey(a => a.TestId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(a => a.Student)
                  .WithMany(u => u.TestAttempts)
                  .HasForeignKey(a => a.StudentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StudentAnswer>(entity =>
        {
            entity.HasKey(sa => sa.Id);

            entity.HasOne(sa => sa.Attempt)
                  .WithMany(a => a.Answers)
                  .HasForeignKey(sa => sa.AttemptId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(sa => sa.Question)
                  .WithMany(q => q.StudentAnswers)
                  .HasForeignKey(sa => sa.QuestionId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LlmCallLog>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.OperationType).HasMaxLength(50).IsRequired();
            entity.Property(l => l.ModelName).HasMaxLength(100).IsRequired();

            entity.HasOne(l => l.Attempt)
                  .WithMany(a => a.LlmCallLogs)
                  .HasForeignKey(l => l.AttemptId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(l => l.Material)
                  .WithMany(m => m.LlmCallLogs)
                  .HasForeignKey(l => l.MaterialId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
