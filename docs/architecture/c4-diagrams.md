# Архітектура системи (C4-модель)

## 1. System Context Diagram (Контекст системи)

```mermaid
C4Context
    title System Context Diagram — Адаптивна освітня RAG-платформа

    Person(student, "Здобувач освіти (Student)", "Проходить адаптивне тестування, отримує персоналізовані пояснення та рекомендації.")
    Person(teacher, "Викладач (Teacher)", "Завантажує навчальні матеріали, налаштовує тести та аналізує результати студентів.")

    System(ragPlatform, "Adaptive RAG Platform", "Вебплатформа для генерації та адаптивної перевірки знань на основі навчальних матеріалів.")

    System_Ext(llmProvider, "External LLM API (OpenAI / Gemini)", "Зовнішній сервіс великих мовних моделей для генерації ембедінгів, тестових завдань та перевірки відповідей.")

    Rel(student, ragPlatform, "Проходить тести, переглядає зворотний зв'язок", "HTTPS")
    Rel(teacher, ragPlatform, "Завантажує конспекти, керує тестами", "HTTPS")
    Rel(ragPlatform, llmProvider, "Надсилає промпти та запити на векторизацію", "REST API / HTTPS")
2. Container Diagram (Контейнери системи)
Фрагмент коду
C4Container
    title Container Diagram — Adaptive RAG Platform (.NET 10)

    Person(user, "Користувач (Student / Teacher)", "Взаємодіє з платформою через браузер.")

    Container_Boundary(c1, "Adaptive RAG Platform") {
        Container(webApp, "Web Application", "Blazor Web App (.NET 10)", "Інтерфейс користувача для проходження тестування та завантаження матеріалів.")
        Container(webApi, "Backend API & Adaptive Engine", "ASP.NET Core Web API (.NET 10)", "REST API, автентифікація, рушій адаптивного підбору складності завдань (1..5).")
        Container(aiModule, "AI & RAG Orchestrator", "Class Library + Semantic Kernel", "Ізольований модуль формування промптів, розбиття тексту на чанки та оркестрації RAG.")
        ContainerDb(postgresDb, "Relational Database", "PostgreSQL 17", "Зберігає користувачів, матеріали, тести, питання, спроби, відповіді та логи викликів LLM.")
        ContainerDb(qdrantDb, "Vector Database", "Qdrant", "Зберігає векторні подання (ембедінги) текстових фрагментів для семантичного пошуку.")
    }

    System_Ext(llmApi, "External LLM Service", "Хмарний провайдер мовних моделей та ембедінгів.")

    Rel(user, webApp, "Використовує", "HTTPS")
    Rel(webApp, webApi, "Викликає ендпоінти", "JSON / HTTPS")
    Rel(webApi, postgresDb, "Читає/записує дані через EF Core", "TCP / SQL")
    Rel(webApi, aiModule, "Викликає генерацію та перевірку", "In-Process (.NET)")
    Rel(aiModule, qdrantDb, "Шукає релевантні чанки та зберігає вектори", "gRPC / HTTP")
    Rel(aiModule, llmApi, "Генерує питання, оцінки та ембедінги", "HTTPS")
