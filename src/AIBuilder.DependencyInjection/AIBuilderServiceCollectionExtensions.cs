using AIBuilder;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering AIBuilder with a <see cref="IServiceCollection"/>.
/// </summary>
public static class AIBuilderServiceCollectionExtensions
{
    /// <summary>
    /// Adds the AIBuilder pipeline to the service collection. Register a provider
    /// (for example <c>UseOpenAI</c>) and optional middleware on the returned builder.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>An <see cref="IAIBuilder"/> for further configuration.</returns>
    public static IAIBuilder AddAIBuilder(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<IAIClient>(sp => new AIClient(
            sp.GetRequiredService<IChatClient>(),
            sp.GetService<IPromptTemplateStore>(),
            sp.GetService<IConversationStore>(),
            sp.GetServices<AITool>()));

        return new DefaultAIBuilder(services);
    }
}
