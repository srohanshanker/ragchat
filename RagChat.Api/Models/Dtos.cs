namespace RagChat.Api.Models;

public record DocumentDto(Guid Id, string FileName, DateTimeOffset UploadedAt, int ChunkCount);

public record ChatRequestDto(string Message);

public record CitationDto(int Index, string DocumentName, int ChunkIndex, double Score, string Excerpt);

public record ChatResponseDto(string Answer, List<CitationDto> Citations);

public record HealthDto(bool OllamaReachable, string BaseUrl, string ChatModel, string EmbeddingModel, string? Detail);
