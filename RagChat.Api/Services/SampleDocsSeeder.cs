using Microsoft.EntityFrameworkCore;
using RagChat.Api.Data;
using RagChat.Api.Models;

namespace RagChat.Api.Services;

/// <summary>
/// Loads the fictional demo docs shipped in wwwroot's sibling "sample-docs" folder, so the
/// chat UI has something to answer questions about without requiring the user's own files.
/// </summary>
public class SampleDocsSeeder
{
    private readonly RagDbContext _db;
    private readonly DocumentIngestionService _ingestion;
    private readonly IWebHostEnvironment _environment;

    public SampleDocsSeeder(RagDbContext db, DocumentIngestionService ingestion, IWebHostEnvironment environment)
    {
        _db = db;
        _ingestion = ingestion;
        _environment = environment;
    }

    public async Task<List<DocumentDto>> SeedAsync(CancellationToken cancellationToken = default)
    {
        var sampleDocsPath = Path.Combine(_environment.ContentRootPath, "sample-docs");
        if (!Directory.Exists(sampleDocsPath))
        {
            throw new DirectoryNotFoundException($"Sample docs folder not found at '{sampleDocsPath}'.");
        }

        var existingNames = await _db.Documents.Select(d => d.FileName).ToListAsync(cancellationToken);
        var results = new List<DocumentDto>();

        foreach (var path in Directory.EnumerateFiles(sampleDocsPath, "*.md"))
        {
            var fileName = Path.GetFileName(path);
            if (existingNames.Contains(fileName))
            {
                continue;
            }

            await using var stream = File.OpenRead(path);
            var dto = await _ingestion.IngestAsync(stream, fileName, "text/markdown", cancellationToken);
            results.Add(dto);
        }

        return results;
    }
}
