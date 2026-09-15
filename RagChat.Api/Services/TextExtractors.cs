using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using UglyToad.PdfPig;

namespace RagChat.Api.Services;

public class PlainTextExtractor : ITextExtractor
{
    private static readonly string[] Extensions = [".txt", ".md"];

    public bool CanHandle(string fileName) =>
        Extensions.Contains(Path.GetExtension(fileName), StringComparer.OrdinalIgnoreCase);

    public async Task<string> ExtractTextAsync(Stream fileStream, string fileName)
    {
        using var reader = new StreamReader(fileStream);
        return await reader.ReadToEndAsync();
    }
}

public class PdfTextExtractor : ITextExtractor
{
    public bool CanHandle(string fileName) =>
        string.Equals(Path.GetExtension(fileName), ".pdf", StringComparison.OrdinalIgnoreCase);

    public Task<string> ExtractTextAsync(Stream fileStream, string fileName)
    {
        using var memory = new MemoryStream();
        fileStream.CopyTo(memory);
        memory.Position = 0;

        using var document = PdfDocument.Open(memory);
        var pages = document.GetPages().Select(p => p.Text);
        return Task.FromResult(string.Join("\n\n", pages));
    }
}

public class DocxTextExtractor : ITextExtractor
{
    public bool CanHandle(string fileName) =>
        string.Equals(Path.GetExtension(fileName), ".docx", StringComparison.OrdinalIgnoreCase);

    public Task<string> ExtractTextAsync(Stream fileStream, string fileName)
    {
        using var memory = new MemoryStream();
        fileStream.CopyTo(memory);
        memory.Position = 0;

        using var wordDoc = WordprocessingDocument.Open(memory, false);
        var body = wordDoc.MainDocumentPart?.Document.Body;
        if (body is null)
        {
            return Task.FromResult(string.Empty);
        }

        var paragraphs = body.Descendants<Paragraph>()
            .Select(p => p.InnerText)
            .Where(text => !string.IsNullOrWhiteSpace(text));

        return Task.FromResult(string.Join("\n\n", paragraphs));
    }
}

public class TextExtractionService
{
    private readonly List<ITextExtractor> _extractors;

    public TextExtractionService()
    {
        _extractors = [new PlainTextExtractor(), new PdfTextExtractor(), new DocxTextExtractor()];
    }

    public bool IsSupported(string fileName) => _extractors.Any(e => e.CanHandle(fileName));

    public async Task<string> ExtractAsync(Stream fileStream, string fileName)
    {
        var extractor = _extractors.FirstOrDefault(e => e.CanHandle(fileName))
            ?? throw new NotSupportedException($"Unsupported file type for '{fileName}'. Supported: .txt, .md, .pdf, .docx");

        var text = await extractor.ExtractTextAsync(fileStream, fileName);
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException($"No extractable text found in '{fileName}'.");
        }

        return text;
    }
}
