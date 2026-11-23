using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdaptiveRag.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CourseMaterials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    SubjectArea = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    RawContent = table.Column<string>(type: "text", nullable: false),
                    IndexingStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    UploadedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseMaterials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseMaterials_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MaterialChunks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MaterialId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChunkIndex = table.Column<int>(type: "integer", nullable: false),
                    ContentText = table.Column<string>(type: "text", nullable: false),
                    QdrantPointId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TokenCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialChunks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaterialChunks_CourseMaterials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "CourseMaterials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MaterialId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    InitialDifficulty = table.Column<int>(type: "integer", nullable: false),
                    MaxQuestions = table.Column<int>(type: "integer", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tests_CourseMaterials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "CourseMaterials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tests_Users_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TestId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceChunkId = table.Column<Guid>(type: "uuid", nullable: true),
                    QuestionText = table.Column<string>(type: "text", nullable: false),
                    QuestionType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    OptionsJson = table.Column<string>(type: "jsonb", nullable: true),
                    CorrectAnswer = table.Column<string>(type: "text", nullable: false),
                    DifficultyLevel = table.Column<int>(type: "integer", nullable: false),
                    IsAiGenerated = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Questions_MaterialChunks_SourceChunkId",
                        column: x => x.SourceChunkId,
                        principalTable: "MaterialChunks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Questions_Tests_TestId",
                        column: x => x.TestId,
                        principalTable: "Tests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TestAttempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TestId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentDifficulty = table.Column<int>(type: "integer", nullable: false),
                    FinalScore = table.Column<double>(type: "double precision", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestAttempts_Tests_TestId",
                        column: x => x.TestId,
                        principalTable: "Tests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TestAttempts_Users_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LlmCallLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AttemptId = table.Column<Guid>(type: "uuid", nullable: true),
                    MaterialId = table.Column<Guid>(type: "uuid", nullable: true),
                    OperationType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ModelName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PromptTokens = table.Column<int>(type: "integer", nullable: false),
                    CompletionTokens = table.Column<int>(type: "integer", nullable: false),
                    LatencyMs = table.Column<int>(type: "integer", nullable: false),
                    IsSuccess = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LlmCallLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LlmCallLogs_CourseMaterials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "CourseMaterials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LlmCallLogs_TestAttempts_AttemptId",
                        column: x => x.AttemptId,
                        principalTable: "TestAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "StudentAnswers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AttemptId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserAnswerText = table.Column<string>(type: "text", nullable: false),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false),
                    ScoreAwarded = table.Column<double>(type: "double precision", nullable: false),
                    AiFeedback = table.Column<string>(type: "text", nullable: true),
                    AnsweredAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentAnswers_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentAnswers_TestAttempts_AttemptId",
                        column: x => x.AttemptId,
                        principalTable: "TestAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseMaterials_AuthorId",
                table: "CourseMaterials",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_LlmCallLogs_AttemptId",
                table: "LlmCallLogs",
                column: "AttemptId");

            migrationBuilder.CreateIndex(
                name: "IX_LlmCallLogs_MaterialId",
                table: "LlmCallLogs",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialChunks_MaterialId",
                table: "MaterialChunks",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialChunks_QdrantPointId",
                table: "MaterialChunks",
                column: "QdrantPointId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Questions_SourceChunkId",
                table: "Questions",
                column: "SourceChunkId");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_TestId",
                table: "Questions",
                column: "TestId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAnswers_AttemptId",
                table: "StudentAnswers",
                column: "AttemptId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAnswers_QuestionId",
                table: "StudentAnswers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_TestAttempts_StudentId",
                table: "TestAttempts",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_TestAttempts_TestId",
                table: "TestAttempts",
                column: "TestId");

            migrationBuilder.CreateIndex(
                name: "IX_Tests_CreatorId",
                table: "Tests",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Tests_MaterialId",
                table: "Tests",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LlmCallLogs");

            migrationBuilder.DropTable(
                name: "StudentAnswers");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "TestAttempts");

            migrationBuilder.DropTable(
                name: "MaterialChunks");

            migrationBuilder.DropTable(
                name: "Tests");

            migrationBuilder.DropTable(
                name: "CourseMaterials");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
