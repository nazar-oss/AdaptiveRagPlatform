using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace KnowledgeBase.Indexer;

public class CohereEmbeddingClient
{
    private const string Model = "embed-multilingual-v3.0";
    private readonly HttpClient _http;

    public CohereEmbeddingClient(string apiKey)
    {
        _http = new HttpClient { BaseAddress = new Uri("https://api.cohere.com/") };
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    }

    public async Task<float[][]> EmbedAsync(IReadOnlyList<string> texts, string inputType)
    {
        var body = new
        {
            model = Model,
            texts,
            input_type = inputType,
            embedding_types = new[] { "float" }
        };

        var response = await _http.PostAsJsonAsync("v2/embed", body);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Cohere API error {(int)response.StatusCode}: {err}");
        }

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var items = doc.RootElement.GetProperty("embeddings").GetProperty("float");

        return items.EnumerateArray()
            .Select(v => v.EnumerateArray().Select(x => x.GetSingle()).ToArray())
            .ToArray();
    }
}