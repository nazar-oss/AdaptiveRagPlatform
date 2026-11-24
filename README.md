# AdaptiveRagPlatform — База знань проєкту (С.2.4)

Модуль індексації та семантичного пошуку (`src/KnowledgeBase.Indexer`) для освітньої платформи **AdaptiveRagPlatform**.

## Параметри системи
- **Модель embeddings:** `embed-multilingual-v3.0` (Cohere API, розмірність векторів `1024`, типи: `search_document` / `search_query`)
- **Векторна БД:** DuckDB (`FLOAT[1024]`, метрика — `array_cosine_distance`)
- **Оптимальний chunking:** розмір `700` символів, перекриття `100` символів (поділ за межами абзаців та речень)
- **Якість пошуку (eval):** `Hit@3 = 7/7 (100%)`, середня позиція першого релевантного чанка — `1.00`

## Запуск
1. Задати змінну середовища з ключем Cohere (ключ не зберігається в репозиторії):
   $env:COHERE_API_KEY="ваш_ключ"
2. Виконати індексацію документів та запуск тестування:
   cd src/KnowledgeBase.Indexer
   dotnet run -- index 700 100
   dotnet run -- eval