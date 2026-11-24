namespace KnowledgeBase.Indexer;

public static class TextChunker
{
    public static List<string> Split(string text, int size, int overlap)
    {
        var chunks = new List<string>();
        text = text.Replace("\r\n", "\n").Trim();
        int start = 0;

        while (start < text.Length)
        {
            int end = Math.Min(start + size, text.Length);

            if (end < text.Length)
            {
                int minEnd = start + size * 7 / 10;
                int cut = text.LastIndexOf("\n\n", end - 1, end - minEnd, StringComparison.Ordinal);
                if (cut < 0)
                    cut = text.LastIndexOf(". ", end - 1, end - minEnd, StringComparison.Ordinal);
                if (cut >= 0) end = cut + 1;
            }

            var piece = text.Substring(start, end - start).Trim();
            if (piece.Length > 0) chunks.Add(piece);

            if (end >= text.Length) break;
            start = Math.Max(end - overlap, start + 1);
        }
        return chunks;
    }
}