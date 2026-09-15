namespace RagChat.Api.Services;

public interface ITextExtractor
{
    bool CanHandle(string fileName);

    Task<string> ExtractTextAsync(Stream fileStream, string fileName);
}
