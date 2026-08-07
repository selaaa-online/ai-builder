using AIBuilder;
using Microsoft.Extensions.AI;
using OpenAI;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// OpenAI provider registration extensions for <see cref="IAIBuilder"/>.
/// </summary>
public static class OpenAIAIBuilderExtensions
{
    private const string DefaultModelId = "gpt-4o-mini";

    /// <summary>
    /// Registers OpenAI as the AI provider for the pipeline.
    /// </summary>
    /// <param name="builder">The AIBuilder instance.</param>
    /// <param name="apiKey">The OpenAI API key.</param>
    /// <param name="modelId">The model identifier. Defaults to <c>gpt-4o-mini</c>.</param>
    public static IAIBuilder UseOpenAI(this IAIBuilder builder, string apiKey, string modelId = DefaultModelId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(modelId);

        IChatClient chatClient = new OpenAIClient(apiKey)
            .GetChatClient(modelId)
            .AsIChatClient();

        return builder.UseChatClient(chatClient);
    }

    /// <summary>
    /// Registers OpenAI using a pre-configured <see cref="OpenAIClient"/>.
    /// </summary>
    /// <param name="builder">The AIBuilder instance.</param>
    /// <param name="client">A configured <see cref="OpenAIClient"/>.</param>
    /// <param name="modelId">The model identifier. Defaults to <c>gpt-4o-mini</c>.</param>
    public static IAIBuilder UseOpenAI(this IAIBuilder builder, OpenAIClient client, string modelId = DefaultModelId)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentException.ThrowIfNullOrWhiteSpace(modelId);

        IChatClient chatClient = client.GetChatClient(modelId).AsIChatClient();
        return builder.UseChatClient(chatClient);
    }
}
