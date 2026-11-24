using System.Globalization;
using DuckDB.NET.Data;

namespace KnowledgeBase.Indexer;

public record Hit(string Source, int Index, string Content, double Distance);

public class KbRepository : IDisposable
{
    private readonly DuckDBConnection _conn;

    public KbRepository(string dbPath = "kb.duckdb")
    {
        _conn = new DuckDBConnection($"Data Source={dbPath}");
        _conn.Open();
        EnsureSchema();
    }

    private void EnsureSchema()
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS chunks (
                id INTEGER PRIMARY KEY,
                source VARCHAR NOT NULL,
                chunk_index INTEGER NOT NULL,
                content VARCHAR NOT NULL,
                embedding FLOAT[1024] NOT NULL
            );
            """;
        cmd.ExecuteNonQuery();
    }

    public void ClearAll()
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "DELETE FROM chunks;";
        cmd.ExecuteNonQuery();
    }

    public void InsertBatch(int startId, string source, List<string> contents, float[][] vectors)
    {
        for (int i = 0; i < contents.Count; i++)
        {
            var vecLiteral = "[" + string.Join(",", vectors[i].Select(v => v.ToString("G9", CultureInfo.InvariantCulture))) + "]::FLOAT[1024]";
            using var cmd = _conn.CreateCommand();
            cmd.CommandText = $"INSERT INTO chunks (id, source, chunk_index, content, embedding) VALUES (?, ?, ?, ?, {vecLiteral});";
            cmd.Parameters.Add(new DuckDBParameter(startId + i));
            cmd.Parameters.Add(new DuckDBParameter(source));
            cmd.Parameters.Add(new DuckDBParameter(i));
            cmd.Parameters.Add(new DuckDBParameter(contents[i]));
            cmd.ExecuteNonQuery();
        }
    }

    public List<Hit> SearchTopK(float[] queryVector, int topK)
    {
        var qLiteral = "[" + string.Join(",", queryVector.Select(v => v.ToString("G9", CultureInfo.InvariantCulture))) + "]::FLOAT[1024]";
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = $"""
            SELECT source, chunk_index, content, array_cosine_distance(embedding, {qLiteral}) AS dist
            FROM chunks
            ORDER BY dist ASC
            LIMIT {topK};
            """;

        var hits = new List<Hit>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            hits.Add(new Hit(
                reader.GetString(0),
                reader.GetInt32(1),
                reader.GetString(2),
                reader.GetDouble(3)
            ));
        }
        return hits;
    }

    public void PrintStats()
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "SELECT source, COUNT(*) AS cnt, array_length(FIRST(embedding)) AS dims FROM chunks GROUP BY source ORDER BY source;";
        using var reader = cmd.ExecuteReader();
        Console.WriteLine("\n--- Перевірка векторної БД (DuckDB) ---");
        Console.WriteLine("source | chunks_count | vector_dims");
        int total = 0;
        while (reader.Read())
        {
            var src = reader.GetString(0);
            var cnt = reader.GetInt32(1);
            var dims = reader.GetInt32(2);
            total += cnt;
            Console.WriteLine($"{src} | {cnt} | {dims}");
        }
        Console.WriteLine($"Всього записів у таблиці chunks: {total}");
    }

    public void Dispose() => _conn.Dispose();
}