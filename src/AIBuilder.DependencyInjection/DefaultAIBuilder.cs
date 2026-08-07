using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

namespace AIBuilder;

/// <summary>
/// Default <see cref="IAIBuilder"/> implementation. Holds the underlying
/// <see cref="ChatClientBuilder"/> once a provider has been registered.
/// </summary>
internal sealed class DefaultAIBuilder : IAIBuilder
{
    public DefaultAIBuilder(IServiceCollection services) => Services = services;

    public IServiceCollection Services { get; }

    /// <summary>The chat-client pipeline builder, set by a provider (e.g. <c>UseOpenAI</c>).</summary>
    public ChatClientBuilder? ChatClientBuilder { get; set; }
}
