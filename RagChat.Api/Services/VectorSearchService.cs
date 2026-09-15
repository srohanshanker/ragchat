using Microsoft.EntityFrameworkCore;
using RagChat.Api.Data;

namespace RagChat.Api.Services;

public record ScoredChunk(Guid DocumentId, string DocumentName, int ChunkIndex, string Content, double Score);

public class VectorSearchService
{
    private readonly RagDbContext _db;

    public VectorSearchService(RagDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Brute-force cosine similarity over every stored chunk. Fine for a personal-scale
    /// demo corpus; a production index would use a real vector index instead of scanning.
    /// </summary>
    public async Task<List<ScoredChunk>> SearchTopKAsync(float[] queryEmbedding, int topK, CancellationToken cancellationToken = default)
    {
        var chunks = await _db.DocumentChunks
            .Include(c => c.Document)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return chunks
            .Select(c => new ScoredChunk(
                c.DocumentId,
                c.Document?.FileName ?? "unknown",
                c.ChunkIndex,
                c.Content,
                VectorMath.CosineSimilarity(queryEmbedding, VectorMath.FromBytes(c.Embedding))))
            .OrderByDescending(s => s.Score)
            .Take(topK)
            .ToList();
    }
}
