using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RagChat.Api.Data;
using RagChat.Api.Models;
using RagChat.Api.Services;

namespace RagChat.Api.Controllers;

[ApiController]
[Route("api/documents")]
public class DocumentsController : ControllerBase
{
    private readonly RagDbContext _db;
    private readonly DocumentIngestionService _ingestion;
    private readonly SampleDocsSeeder _seeder;
    private readonly RagOptions _options;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(
        RagDbContext db,
        DocumentIngestionService ingestion,
        SampleDocsSeeder seeder,
        Microsoft.Extensions.Options.IOptions<RagOptions> options,
        ILogger<DocumentsController> logger)
    {
        _db = db;
        _ingestion = ingestion;
        _seeder = seeder;
        _options = options.Value;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<DocumentDto>>> List(CancellationToken cancellationToken)
    {
        var documents = await _db.Documents
            .AsNoTracking()
            .Select(d => new DocumentDto(d.Id, d.FileName, d.UploadedAt, d.ChunkCount))
            .ToListAsync(cancellationToken);

        return Ok(documents.OrderByDescending(d => d.UploadedAt).ToList());
    }

    [HttpPost]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<ActionResult<DocumentDto>> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("No file was provided.");
        }

        if (file.Length > _options.MaxUploadBytes)
        {
            return BadRequest($"File exceeds the {_options.MaxUploadBytes / (1024 * 1024)} MB limit.");
        }

        try
        {
            await using var stream = file.OpenReadStream();
            var result = await _ingestion.IngestAsync(stream, file.FileName, file.ContentType, cancellationToken);
            return Ok(result);
        }
        catch (NotSupportedException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (OllamaUnavailableException ex)
        {
            _logger.LogWarning(ex, "Ollama unavailable during ingestion");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, ex.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var document = await _db.Documents.FindAsync([id], cancellationToken);
        if (document is null)
        {
            return NotFound();
        }

        _db.Documents.Remove(document);
        await _db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("seed-samples")]
    public async Task<ActionResult<List<DocumentDto>>> SeedSamples(CancellationToken cancellationToken)
    {
        try
        {
            var seeded = await _seeder.SeedAsync(cancellationToken);
            return Ok(seeded);
        }
        catch (OllamaUnavailableException ex)
        {
            _logger.LogWarning(ex, "Ollama unavailable during sample seeding");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, ex.Message);
        }
    }
}
