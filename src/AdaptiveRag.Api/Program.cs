using AdaptiveRag.Api.Ai;
using AdaptiveRag.Data;
using AdaptiveRag.Data.Entities;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AdaptiveRagDbContext>(opt =>
    opt.UseInMemoryDatabase("AdaptiveRagAiDb"));

builder.Services.Configure<AiOptions>(builder.Configuration.GetSection("Ai"));
builder.Services.AddScoped<StudyAssistant>();
builder.Services.AddScoped<PromptEvaluator>();

var app = builder.Build();

// Заповнення тестових даних для перевірки function calling (матеріали 101 та 102)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AdaptiveRagDbContext>();
    if (!db.Users.Any())
    {
        var teacher = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Бац Б. М.",
            Email = "teacher@college.edu.ua",
            Role = "Teacher"
        };
        db.Users.Add(teacher);

        db.CourseMaterials.AddRange(
            new CourseMaterial
            {
                Id = Guid.NewGuid(),
                AuthorId = teacher.Id,
                Title = "101. Архітектура та патерни проєктування ПЗ",
                SubjectArea = "Інженерія програмного забезпечення",
                IndexingStatus = "Indexed"
            },
            new CourseMaterial
            {
                Id = Guid.NewGuid(),
                AuthorId = teacher.Id,
                Title = "102. Векторні бази даних та RAG-системи",
                SubjectArea = "Штучний інтелект",
                IndexingStatus = "Indexed"
            }
        );
        db.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapPost("/api/ai/assistant", async (AssistantRequest r, StudyAssistant ai) =>
    Results.Ok(await ai.HandleAsync(r.Text)));

if (args.Contains("--eval"))
{
    using var evalScope = app.Services.CreateScope();
    await evalScope.ServiceProvider.GetRequiredService<PromptEvaluator>().RunAsync();
    return;
}

app.Run();

public record AssistantRequest(string Text);
