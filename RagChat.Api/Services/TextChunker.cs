namespace RagChat.Api.Services;

public class TextChunker
{
    /// <summary>
    /// Splits text into overlapping word-count windows. A real tokenizer would count
    /// model tokens rather than words, but word count keeps this dependency-free and
    /// close enough for a demo-scale corpus.
    /// </summary>
    public List<string> Chunk(string text, int maxWords = 250, int overlapWords = 50)
    {
        if (maxWords <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxWords), "maxWords must be positive.");
        }

        if (overlapWords < 0 || overlapWords >= maxWords)
        {
            throw new ArgumentOutOfRangeException(nameof(overlapWords), "overlapWords must be non-negative and smaller than maxWords.");
        }

        var words = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0)
        {
            return [];
        }

        var chunks = new List<string>();
        var step = maxWords - overlapWords;

        for (var start = 0; start < words.Length; start += step)
        {
            var length = Math.Min(maxWords, words.Length - start);
            chunks.Add(string.Join(' ', words.Skip(start).Take(length)));

            if (start + length >= words.Length)
            {
                break;
            }
        }

        return chunks;
    }
}
