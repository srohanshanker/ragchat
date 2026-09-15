using Microsoft.AspNetCore.Mvc;
using RagChat.Api.Models;
using RagChat.Api.Services;

namespace RagChat.Api.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly RagChatService _ragChatService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(RagChatService ragChatService, ILogger<ChatController> logger)
    {
        _ragChatService = ragChatService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<ChatResponseDto>> Post([FromBody] ChatRequestDto request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest("Message is required.");
        }

        try
        {
            var response = await _ragChatService.AskAsync(request.Message, cancellationToken);
            return Ok(response);
        }
        catch (OllamaUnavailableException ex)
        {
            _logger.LogWarning(ex, "Ollama unavailable during chat");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, ex.Message);
        }
    }
}
