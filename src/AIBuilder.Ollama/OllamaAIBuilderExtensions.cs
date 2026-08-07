using AIBuilder;
using Microsoft.Extensions.AI;
using OllamaSharp;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Ollama provider registration extensions for <see cref="IAIBuilder"/>.
/// </summary>
public static class OllamaAIBuilderExtensions
{
    private const string DefaultEndpoint = "http://localhost:11434";

    /// <summary>
    /// Registers a local Ollama instance as the AI provider.
    /// </summary>
    /// <param name="builder">The AIBuilder instance.</param>
    /// <param name="model">The Ollama model name (for example <c>llama3.2</c>).</param>
    /// <param name="endpoint">The Ollama endpoint. Defaults to <c>http://localhost:11434</c>.</param>
    public static IAIBuilder UseOllama(this IAIBuilder builder, string model, string endpoint = DefaultEndpoint)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(model);
        ArgumentException.ThrowIfNullOrWhiteSpace(endpoint);

        IChatClient chatClient = new OllamaApiClient(new Uri(endpoint), model);
        return builder.UseChatClient(chatClient);
    }
}
