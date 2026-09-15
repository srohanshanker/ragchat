using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RagChat.Api.Models;
using RagChat.Api.Services;

namespace RagChat.Api.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    private readonly OllamaClient _ollamaClient;
    private readonly OllamaOptions _options;

    public HealthController(OllamaClient ollamaClient, IOptions<OllamaOptions> options)
    {
        _ollamaClient = ollamaClient;
        _options = options.Value;
    }

    [HttpGet]
    public async Task<ActionResult<HealthDto>> Get(CancellationToken cancellationToken)
    {
        var reachable = await _ollamaClient.IsReachableAsync(cancellationToken);
        var detail = reachable
            ? null
            : $"Ollama not reachable at {_options.BaseUrl}. Install it from https://ollama.com, run 'ollama serve', " +
              $"then 'ollama pull {_options.ChatModel}' and 'ollama pull {_options.EmbeddingModel}'.";

        return Ok(new HealthDto(reachable, _options.BaseUrl, _options.ChatModel, _options.EmbeddingModel, detail));
    }
}
