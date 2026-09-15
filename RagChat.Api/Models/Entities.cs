namespace RagChat.Api.Models;

public class DocumentEntity
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public DateTimeOffset UploadedAt { get; set; }
    public int ChunkCount { get; set; }

    public List<DocumentChunkEntity> Chunks { get; set; } = new();
}

public class DocumentChunkEntity
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public int ChunkIndex { get; set; }
    public string Content { get; set; } = string.Empty;
    public byte[] Embedding { get; set; } = Array.Empty<byte>();

    public DocumentEntity? Document { get; set; }
}
