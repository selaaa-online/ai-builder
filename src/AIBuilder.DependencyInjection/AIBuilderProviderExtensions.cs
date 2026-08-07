using AIBuilder;
using Microsoft.Extensions.AI;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provider-facing extensions for registering the underlying chat client pipeline.
/// </summary>
public static class AIBuilderProviderExtensions
{
    /// <summary>
    /// Registers the given <see cref="IChatClient"/> as the provider for the pipeline.
    /// Intended for use by provider packages (for example <c>UseOpenAI</c>).
    /// </summary>
    /// <param name="builder">The AIBuilder instance.</param>
    /// <param name="chatClient">The provider chat client to place at the end of the pipeline.</param>
    public static IAIBuilder UseChatClient(this IAIBuilder builder, IChatClient chatClient)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(chatClient);

        if (builder is not DefaultAIBuilder defaultBuilder)
        {
            throw new InvalidOperationException(
                $"The builder must be created by AddAIBuilder(). Actual type: {builder.GetType().FullName}.");
        }

        defaultBuilder.ChatClientBuilder = builder.Services.AddChatClient(chatClient);
        return builder;
    }

    /// <summary>
    /// Gets the underlying <see cref="ChatClientBuilder"/>, throwing if no provider has been registered.
    /// Intended for middleware and provider packages.
    /// </summary>
    public static ChatClientBuilder RequireChatClientBuilder(this IAIBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (builder is not DefaultAIBuilder { ChatClientBuilder: { } chatClientBuilder })
        {
            throw new InvalidOperationException(
                "No AI provider has been registered. Call a provider method such as UseOpenAI() before adding middleware.");
        }

        return chatClientBuilder;
    }
}
