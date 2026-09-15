using Microsoft.Extensions.Options;
using RagChat.Api.Models;

namespace RagChat.Api.Services;

public class RagChatService
{
    private const string SystemPromptTemplate =
        "You are a helpful assistant that answers questions using ONLY the numbered context " +
        "excerpts below. Cite the excerpt number(s) you used like [1] or [2][3]. If the context " +
        "does not contain the answer, say you don't know rather than guessing.\n\n{0}";

    private readonly VectorSearchService _vectorSearch;
    private readonly IEmbeddingService _embeddingService;
    private readonly IChatCompletionService _chatService;
    private readonly RagOptions _options;

    public RagChatService(
        VectorSearchService vectorSearch,
        IEmbeddingService embeddingService,
        IChatCompletionService chatService,
        IOptions<RagOptions> options)
    {
        _vectorSearch = vectorSearch;
        _embeddingService = embeddingService;
        _chatService = chatService;
        _options = options.Value;
    }

    public async Task<ChatResponseDto> AskAsync(string question, CancellationToken cancellationToken = default)
    {
        var queryEmbedding = await _embeddingService.EmbedAsync(question, cancellationToken);
        var topChunks = await _vectorSearch.SearchTopKAsync(queryEmbedding, _options.TopK, cancellationToken);

        if (topChunks.Count == 0)
        {
            return new ChatResponseDto(
                "No documents have been uploaded yet, so I have nothing to answer from. Upload a document first.",
                []);
        }

        var citations = topChunks
            .Select((chunk, index) => new CitationDto(
                index + 1,
                chunk.DocumentName,
                chunk.ChunkIndex,
                Math.Round(chunk.Score, 4),
                Excerpt(chunk.Content)))
            .ToList();

        var context = string.Join("\n\n", citations.Select(c =>
            $"[{c.Index}] (from \"{c.DocumentName}\", chunk {c.ChunkIndex}):\n{topChunks[c.Index - 1].Content}"));

        var systemPrompt = string.Format(SystemPromptTemplate, context);
        var answer = await _chatService.CompleteAsync(systemPrompt, question, cancellationToken);

        return new ChatResponseDto(answer, citations);
    }

    private static string Excerpt(string content, int maxChars = 200) =>
        content.Length <= maxChars ? content : content[..maxChars] + "…";
}
