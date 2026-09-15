using Microsoft.Extensions.Options;
using RagChat.Api.Data;
using RagChat.Api.Models;

namespace RagChat.Api.Services;

public class DocumentIngestionService
{
    private readonly RagDbContext _db;
    private readonly TextExtractionService _extractor;
    private readonly TextChunker _chunker;
    private readonly IEmbeddingService _embeddingService;
    private readonly RagOptions _options;
    private readonly ILogger<DocumentIngestionService> _logger;

    public DocumentIngestionService(
        RagDbContext db,
        TextExtractionService extractor,
        TextChunker chunker,
        IEmbeddingService embeddingService,
        IOptions<RagOptions> options,
        ILogger<DocumentIngestionService> logger)
    {
        _db = db;
        _extractor = extractor;
        _chunker = chunker;
        _embeddingService = embeddingService;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<DocumentDto> IngestAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        if (!_extractor.IsSupported(fileName))
        {
            throw new NotSupportedException($"Unsupported file type for '{fileName}'. Supported: .txt, .md, .pdf, .docx");
        }

        var text = await _extractor.ExtractAsync(fileStream, fileName);
        var chunks = _chunker.Chunk(text, _options.ChunkSizeWords, _options.ChunkOverlapWords);

        if (chunks.Count == 0)
        {
            throw new InvalidOperationException($"'{fileName}' produced no chunks after extraction.");
        }

        var document = new DocumentEntity
        {
            Id = Guid.NewGuid(),
            FileName = fileName,
            ContentType = contentType,
            UploadedAt = DateTimeOffset.UtcNow,
            ChunkCount = chunks.Count,
        };

        _logger.LogInformation("Ingesting '{FileName}' as {ChunkCount} chunks", fileName, chunks.Count);

        for (var i = 0; i < chunks.Count; i++)
        {
            var embedding = await _embeddingService.EmbedAsync(chunks[i], cancellationToken);
            document.Chunks.Add(new DocumentChunkEntity
            {
                Id = Guid.NewGuid(),
                DocumentId = document.Id,
                ChunkIndex = i,
                Content = chunks[i],
                Embedding = VectorMath.ToBytes(embedding),
            });
        }

        _db.Documents.Add(document);
        await _db.SaveChangesAsync(cancellationToken);

        return new DocumentDto(document.Id, document.FileName, document.UploadedAt, document.ChunkCount);
    }
}
