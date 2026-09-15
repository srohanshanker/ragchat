namespace RagChat.Api.Services;

public class RagOptions
{
    public const string SectionName = "Rag";

    public int ChunkSizeWords { get; set; } = 250;
    public int ChunkOverlapWords { get; set; } = 50;
    public int TopK { get; set; } = 4;
    public long MaxUploadBytes { get; set; } = 10 * 1024 * 1024;
}
