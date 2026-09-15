using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace RagChat.Api.Services;

/// <summary>
/// Talks to a locally running Ollama daemon (https://ollama.com) over its REST API.
/// No API key, no cloud account — Ollama must simply be running on the configured BaseUrl.
/// </summary>
public class OllamaClient : IEmbeddingService, IChatCompletionService
{
    private readonly HttpClient _httpClient;
    private readonly OllamaOptions _options;

    public OllamaClient(HttpClient httpClient, IOptions<OllamaOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
    }

    public async Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)
    {
        var request = new EmbedRequest(_options.EmbeddingModel, text);

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsJsonAsync("/api/embed", request, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new OllamaUnavailableException(_options.BaseUrl, ex);
        }

        await EnsureSuccessAsync(response, cancellationToken);

        var body = await response.Content.ReadFromJsonAsync<EmbedResponse>(cancellationToken: cancellationToken);
        var embedding = body?.Embeddings?.FirstOrDefault();
        if (embedding is null || embedding.Length == 0)
        {
            throw new InvalidOperationException($"Ollama returned no embedding for model '{_options.EmbeddingModel}'.");
        }

        return embedding;
    }

    public async Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
    {
        var request = new ChatRequest(
            _options.ChatModel,
            [
                new ChatMessage("system", systemPrompt),
                new ChatMessage("user", userPrompt),
            ],
            Stream: false);

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsJsonAsync("/api/chat", request, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new OllamaUnavailableException(_options.BaseUrl, ex);
        }

        await EnsureSuccessAsync(response, cancellationToken);

        var body = await response.Content.ReadFromJsonAsync<ChatResponse>(cancellationToken: cancellationToken);
        var answer = body?.Message?.Content;
        if (string.IsNullOrWhiteSpace(answer))
        {
            throw new InvalidOperationException($"Ollama returned an empty response from model '{_options.ChatModel}'.");
        }

        return answer;
    }

    public async Task<bool> IsReachableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/version", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }

    private async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new InvalidOperationException(
                $"Ollama model not found (model '{_options.ChatModel}' / '{_options.EmbeddingModel}'). " +
                $"Run 'ollama pull {_options.ChatModel}' and 'ollama pull {_options.EmbeddingModel}'. Detail: {body}");
        }

        throw new InvalidOperationException($"Ollama request failed ({(int)response.StatusCode}): {body}");
    }

    private record EmbedRequest([property: JsonPropertyName("model")] string Model, [property: JsonPropertyName("input")] string Input);

    private record EmbedResponse([property: JsonPropertyName("embeddings")] List<float[]>? Embeddings);

    private record ChatMessage([property: JsonPropertyName("role")] string Role, [property: JsonPropertyName("content")] string Content);

    private record ChatRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("messages")] List<ChatMessage> Messages,
        [property: JsonPropertyName("stream")] bool Stream);

    private record ChatResponse([property: JsonPropertyName("message")] ChatMessage? Message);
}

public class OllamaUnavailableException : Exception
{
    public OllamaUnavailableException(string baseUrl, Exception inner)
        : base($"Could not reach Ollama at {baseUrl}. Is it running? Start it with 'ollama serve' (or the Ollama app) and make sure the required models are pulled.", inner)
    {
    }
}
