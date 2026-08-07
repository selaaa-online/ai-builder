using AIBuilder;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Caching.Distributed;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Response caching registration extensions for <see cref="IAIBuilder"/>.
/// </summary>
public static class AIBuilderCacheExtensions
{
    /// <summary>
    /// Adds an in-process distributed memory cache and caches responses in it.
    /// </summary>
    public static IAIBuilder UseMemoryCache(this IAIBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Services.AddDistributedMemoryCache();
        return builder.UseDistributedCache();
    }

    /// <summary>
    /// Caches responses using the <see cref="IDistributedCache"/> registered in the container
    /// (for example Redis or SQL Server distributed cache).
    /// </summary>
    public static IAIBuilder UseDistributedCache(this IAIBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.RequireChatClientBuilder().Use((inner, services) =>
            new DistributedCachingChatClient(inner, services.GetRequiredService<IDistributedCache>()));

        return builder;
    }
}
