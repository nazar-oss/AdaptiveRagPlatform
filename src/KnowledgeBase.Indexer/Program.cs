using System.Text;
using KnowledgeBase.Indexer;

Console.OutputEncoding = Encoding.UTF8;

var command = args.Length > 0 ? args[0] : "";
int size = args.Length > 1 ? int.Parse(args[1]) : 700;
int overlap = args.Length > 2 ? int.Parse(args[2]) : 100;

if (command == "stats")
{
    using var repo = new KbRepository();
    repo.PrintStats();
    return;
}

var key = Environment.GetEnvironmentVariable("COHERE_API_KEY")
    ?? throw new InvalidOperationException("COHERE_API_KEY is not set");
var cohere = new CohereEmbeddingClient(key);

if (command == "index")
    await IndexAsync(cohere, "docs", size, overlap);
else if (command == "eval")
    await EvalAsync(cohere, "queries.csv");
else if (command == "search" && args.Length > 1)
    await SingleSearchAsync(cohere, string.Join(" ", args.Skip(1)));
else
    Console.WriteLine("Usage: index [size] [overlap] | eval | stats | search <query>");

static async Task IndexAsync(CohereEmbeddingClient cohere, string folder, int size, int overlap)
{
    using var repo = new KbRepository();
    repo.ClearAll();

    var files = Directory.GetFiles(folder)
        .Where(f => f.EndsWith(".txt") || f.EndsWith(".md"))
        .OrderBy(f => f)
        .ToList();

    int nextId = 1;
    var allLengths = new List<int>();
    var sampleChunks = new List<(string Src, int Idx, string Text)>();

    Console.WriteLine($"Індексація (size={size}, overlap={overlap}):");
    foreach (var file in files)
    {
        var text = await File.ReadAllTextAsync(file, Encoding.UTF8);
        var chunks = TextChunker.Split(text, size, overlap);
        var name = Path.GetFileName(file);

        for (int i = 0; i < chunks.Count; i += 96)
        {
            var batch = chunks.Skip(i).Take(96).ToList();
            var vectors = await cohere.EmbedAsync(batch, "search_document");
            repo.InsertBatch(nextId, name, batch, vectors);
            nextId += batch.Count;
        }

        allLengths.AddRange(chunks.Select(c => c.Length));
        if (chunks.Count > 0) sampleChunks.Add((name, 0, chunks[0]));
        Console.WriteLine($"  {name}: {chunks.Count} чанків (символів: {text.Length})");
    }

    Console.WriteLine($"\nСтатистика чанків: всього={allLengths.Count}, min={allLengths.Min()}, max={allLengths.Max()}, avg={allLengths.Average():F1}");
    repo.PrintStats();
}

static async Task SingleSearchAsync(CohereEmbeddingClient cohere, string query)
{
    using var repo = new KbRepository();
    var qVec = (await cohere.EmbedAsync(new[] { query }, "search_query"))[0];
    var hits = repo.SearchTopK(qVec, 3);
    Console.WriteLine($"Запит: {query}");
    foreach (var h in hits)
        Console.WriteLine($"  [{h.Source} #{h.Index}] distance={h.Distance:F4} | {h.Content.Replace("\n", " ")[..Math.Min(90, h.Content.Length)]}...");
}

static async Task EvalAsync(CohereEmbeddingClient cohere, string csvPath)
{
    using var repo = new KbRepository();
    var rows = File.ReadAllLines(csvPath, Encoding.UTF8).Skip(1)
        .Where(l => !string.IsNullOrWhiteSpace(l))
        .ToList();

    var questions = rows.Select(r => r.Split(';')[0].Trim()).ToList();
    var qVectors = await cohere.EmbedAsync(questions, "search_query");

    int answerable = 0, hits = 0;
    for (int i = 0; i < rows.Count; i++)
    {
        var parts = rows[i].Split(';');
        var expected = parts.Length > 1
            ? parts[1].Trim().Split('|', StringSplitOptions.RemoveEmptyEntries)
            : Array.Empty<string>();

        var found = repo.SearchTopK(qVectors[i], 3);
        Console.WriteLine($"\nQ{i + 1}: {parts[0]} (очікується: {(expected.Length == 0 ? "НЕМАЄ ВІДПОВІДІ" : string.Join(", ", expected))})");
        for (int rank = 0; rank < found.Count; rank++)
        {
            var h = found[rank];
            var preview = h.Content.Replace("\n", " ");
            Console.WriteLine($"  #{rank + 1} {h.Source} [chunk {h.Index}] d={h.Distance:F4} -> {preview[..Math.Min(85, preview.Length)]}...");
        }

        if (expected.Length > 0)
        {
            answerable++;
            if (found.Any(x => expected.Contains(x.Source))) hits++;
        }
    }
    Console.WriteLine($"\nHit@3 (by source): {hits}/{answerable} ({(double)hits / answerable * 100:F1}%)");
}