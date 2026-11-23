CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261123175810_InitialCreate') THEN
    CREATE TABLE "Users" (
        "Id" uuid NOT NULL,
        "FullName" character varying(150) NOT NULL,
        "Email" character varying(200) NOT NULL,
        "PasswordHash" text NOT NULL,
        "Role" character varying(30) NOT NULL,
        "CreatedAtUtc" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261123175810_InitialCreate') THEN
    CREATE TABLE "CourseMaterials" (
        "Id" uuid NOT NULL,
        "AuthorId" uuid NOT NULL,
        "Title" character varying(250) NOT NULL,
        "SubjectArea" character varying(120) NOT NULL,
        "RawContent" text NOT NULL,
        "IndexingStatus" character varying(30) NOT NULL,
        "UploadedAtUtc" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_CourseMaterials" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_CourseMaterials_Users_AuthorId" FOREIGN KEY ("AuthorId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261123175810_InitialCreate') THEN
    CREATE TABLE "MaterialChunks" (
        "Id" uuid NOT NULL,
        "MaterialId" uuid NOT NULL,
        "ChunkIndex" integer NOT NULL,
        "ContentText" text NOT NULL,
        "QdrantPointId" character varying(100) NOT NULL,
        "TokenCount" integer NOT NULL,
        CONSTRAINT "PK_MaterialChunks" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_MaterialChunks_CourseMaterials_MaterialId" FOREIGN KEY ("MaterialId") REFERENCES "CourseMaterials" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261123175810_InitialCreate') THEN
    CREATE TABLE "Tests" (
        "Id" uuid NOT NULL,
        "MaterialId" uuid NOT NULL,
        "CreatorId" uuid NOT NULL,
        "Title" character varying(250) NOT NULL,
        "InitialDifficulty" integer NOT NULL,
        "MaxQuestions" integer NOT NULL,
        "IsPublished" boolean NOT NULL,
        "CreatedAtUtc" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Tests" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Tests_CourseMaterials_MaterialId" FOREIGN KEY ("MaterialId") REFERENCES "CourseMaterials" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_Tests_Users_CreatorId" FOREIGN KEY ("CreatorId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261123175810_InitialCreate') THEN
    CREATE TABLE "Questions" (
        "Id" uuid NOT NULL,
        "TestId" uuid NOT NULL,
        "SourceChunkId" uuid,
        "QuestionText" text NOT NULL,
        "QuestionType" character varying(40) NOT NULL,
        "OptionsJson" jsonb,
        "CorrectAnswer" text NOT NULL,
        "DifficultyLevel" integer NOT NULL,
        "IsAiGenerated" boolean NOT NULL,
        CONSTRAINT "PK_Questions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Questions_MaterialChunks_SourceChunkId" FOREIGN KEY ("SourceChunkId") REFERENCES "MaterialChunks" ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_Questions_Tests_TestId" FOREIGN KEY ("TestId") REFERENCES "Tests" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261123175810_InitialCreate') THEN
    CREATE TABLE "TestAttempts" (
        "Id" uuid NOT NULL,
        "TestId" uuid NOT NULL,
        "StudentId" uuid NOT NULL,
        "CurrentDifficulty" integer NOT NULL,
        "FinalScore" double precision NOT NULL,
        "Status" character varying(30) NOT NULL,
        "StartedAtUtc" timestamp with time zone NOT NULL,
        "CompletedAtUtc" timestamp with time zone,
        CONSTRAINT "PK_TestAttempts" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_TestAttempts_Tests_TestId" FOREIGN KEY ("TestId") REFERENCES "Tests" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_TestAttempts_Users_StudentId" FOREIGN KEY ("StudentId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261123175810_InitialCreate') THEN
    CREATE TABLE "LlmCallLogs" (
        "Id" uuid NOT NULL,
        "AttemptId" uuid,
        "MaterialId" uuid,
        "OperationType" character varying(50) NOT NULL,
        "ModelName" character varying(100) NOT NULL,
        "PromptTokens" integer NOT NULL,
        "CompletionTokens" integer NOT NULL,
        "LatencyMs" integer NOT NULL,
        "IsSuccess" boolean NOT NULL,
        "CreatedAtUtc" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_LlmCallLogs" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_LlmCallLogs_CourseMaterials_MaterialId" FOREIGN KEY ("MaterialId") REFERENCES "CourseMaterials" ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_LlmCallLogs_TestAttempts_AttemptId" FOREIGN KEY ("AttemptId") REFERENCES "TestAttempts" ("Id") ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261123175810_InitialCreate') THEN
    CREATE TABLE "StudentAnswers" (
        "Id" uuid NOT NULL,
        "AttemptId" uuid NOT NULL,
        "QuestionId" uuid NOT NULL,
        "UserAnswerText" text NOT NULL,
        "IsCorrect" boolean NOT NULL,
        "ScoreAwarded" double precision NOT NULL,
        "AiFeedback" text,
        "AnsweredAtUtc" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_StudentAnswers" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_StudentAnswers_Questions_QuestionId" FOREIGN KEY ("QuestionId") REFERENCES "Questions" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_StudentAnswers_TestAttempts_AttemptId" FOREIGN KEY ("AttemptId") REFERENCES "TestAttempts" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261123175810_InitialCreate') THEN
    CREATE INDEX "IX_CourseMaterials_AuthorId" ON "CourseMaterials" ("AuthorId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261123175810_InitialCreate') THEN
    CREATE INDEX "IX_LlmCallLogs_AttemptId" ON "LlmCallLogs" ("AttemptId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261123175810_InitialCreate') THEN
    CREATE INDEX "IX_LlmCallLogs_MaterialId" ON "LlmCallLogs" ("MaterialId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261123175810_InitialCreate') THEN
    CREATE INDEX "IX_MaterialChunks_MaterialId" ON "MaterialChunks" ("MaterialId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261123175810_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_MaterialChunks_QdrantPointId" ON "MaterialChunks" ("QdrantPointId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261123175810_InitialCreate') THEN
    CREATE INDEX "IX_Questions_SourceChunkId" ON "Questions" ("SourceChunkId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261123175810_InitialCreate') THEN
    CREATE INDEX "IX_Questions_TestId" ON "Questions" ("TestId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261123175810_InitialCreate') THEN
    CREATE INDEX "IX_StudentAnswers_AttemptId" ON "StudentAnswers" ("AttemptId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261123175810_InitialCreate') THEN
    CREATE INDEX "IX_StudentAnswers_QuestionId" ON "StudentAnswers" ("QuestionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261123175810_InitialCreate') THEN
    CREATE INDEX "IX_TestAttempts_StudentId" ON "TestAttempts" ("StudentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261123175810_InitialCreate') THEN
    CREATE INDEX "IX_TestAttempts_TestId" ON "TestAttempts" ("TestId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261123175810_InitialCreate') THEN
    CREATE INDEX "IX_Tests_CreatorId" ON "Tests" ("CreatorId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261123175810_InitialCreate') THEN
    CREATE INDEX "IX_Tests_MaterialId" ON "Tests" ("MaterialId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261123175810_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261123175810_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261123175810_InitialCreate', '10.0.15');
    END IF;
END $EF$;
COMMIT;

