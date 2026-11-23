# ER-модель бази даних (PostgreSQL)

## Опис сутностей
1. **User** — користувачі системи (здобувачі освіти, викладачі, адміністратори).
2. **CourseMaterial** — завантажені навчальні матеріали (лекції, конспекти, методички).
3. **MaterialChunk** — текстові фрагменти матеріалу, пов'язані з векторами у Qdrant через `QdrantPointId`.
4. **Test** — налаштування адаптивного тестування за певним курсом/матеріалом.
5. **Question** — банк тестових завдань (як створених викладачем, так і згенерованих RAG-модулем) із рівнем складності `1..5`.
6. **TestAttempt** — сесія проходження тесту студентом із відстеженням поточного рівня складності.
7. **StudentAnswer** — відповіді студента на конкретні питання та згенерований ШІ зворотний зв'язок (`AiFeedback`).
8. **LlmCallLog** — журнал аудиту викликів LLM (операція, модель, токени, затримка).

## Діаграма сутність-зв'язок (Mermaid)

```mermaid
erDiagram
    User ||--o{ CourseMaterial : "uploads (Teacher)"
    User ||--o{ Test : "creates (Teacher)"
    User ||--o{ TestAttempt : "takes (Student)"

    CourseMaterial ||--o{ MaterialChunk : "splits into"
    CourseMaterial ||--o{ Test : "basis for"

    Test ||--o{ Question : "contains"
    Test ||--o{ TestAttempt : "has"

    TestAttempt ||--o{ StudentAnswer : "records"
    Question ||--o{ StudentAnswer : "answered in"

    TestAttempt ||--o{ LlmCallLog : "triggers"
    CourseMaterial ||--o{ LlmCallLog : "indexes via"

    User {
        uuid Id PK
        string FullName
        string Email UK
        string PasswordHash
        string Role
        datetime CreatedAtUtc
    }

    CourseMaterial {
        uuid Id PK
        uuid AuthorId FK
        string Title
        string SubjectArea
        string RawContent
        string IndexingStatus
        datetime UploadedAtUtc
    }

    MaterialChunk {
        uuid Id PK
        uuid MaterialId FK
        int ChunkIndex
        string ContentText
        string QdrantPointId UK
        int TokenCount
    }

    Test {
        uuid Id PK
        uuid MaterialId FK
        uuid CreatorId FK
        string Title
        int InitialDifficulty
        int MaxQuestions
        bool IsPublished
        datetime CreatedAtUtc
    }

    Question {
        uuid Id PK
        uuid TestId FK
        uuid SourceChunkId FK
        string QuestionText
        string QuestionType
        string OptionsJson
        string CorrectAnswer
        int DifficultyLevel
        bool IsAiGenerated
    }

    TestAttempt {
        uuid Id PK
        uuid TestId FK
        uuid StudentId FK
        int CurrentDifficulty
        double FinalScore
        string Status
        datetime StartedAtUtc
        datetime CompletedAtUtc
    }

    StudentAnswer {
        uuid Id PK
        uuid AttemptId FK
        uuid QuestionId FK
        string UserAnswerText
        bool IsCorrect
        double ScoreAwarded
        string AiFeedback
        datetime AnsweredAtUtc
    }

    LlmCallLog {
        uuid Id PK
        uuid AttemptId FK
        uuid MaterialId FK
        string OperationType
        string ModelName
        int PromptTokens
        int CompletionTokens
        int LatencyMs
        bool IsSuccess
        datetime CreatedAtUtc
    }
