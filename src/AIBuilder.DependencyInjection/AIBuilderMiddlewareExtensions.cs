using AIBuilder;
using AIBuilder.Middleware;
using Microsoft.Extensions.AI;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Middleware registration extensions for <see cref="IAIBuilder"/>.
/// </summary>
public static class AIBuilderMiddlewareExtensions
{
    /// <summary>
    /// Adds logging middleware to the pipeline. Requires an <c>ILoggerFactory</c>
    /// to be registered in the service collection.
    /// </summary>
    public static IAIBuilder UseLogging(this IAIBuilder builder)
    {
        builder.RequireChatClientBuilder().UseLogging();
        return builder;
    }

    /// <summary>
    /// Adds retry middleware with exponential backoff to the pipeline.
    /// </summary>
    /// <param name="builder">The AIBuilder instance.</param>
    /// <param name="configure">An optional delegate to configure <see cref="RetryOptions"/>.</param>
    public static IAIBuilder UseRetry(this IAIBuilder builder, Action<RetryOptions>? configure = null)
    {
        var options = new RetryOptions();
        configure?.Invoke(options);

        builder.RequireChatClientBuilder().Use(inner => new RetryChatClient(inner, options));
        return builder;
    }

    /// <summary>
    /// Adds cost-tracking middleware. A <see cref="CostReport"/> is attached to each response's
    /// additional properties under <see cref="CostOptions.ResponsePropertyKey"/>.
    /// </summary>
    public static IAIBuilder UseCost(this IAIBuilder builder, Action<CostOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var options = new CostOptions();
        configure(options);

        builder.RequireChatClientBuilder().Use(inner => new CostChatClient(inner, options));
        return builder;
    }

    /// <summary>
    /// Adds rate-limiting middleware using a sliding window.
    /// </summary>
    public static IAIBuilder UseRateLimit(this IAIBuilder builder, Action<RateLimitOptions>? configure = null)
    {
        var options = new RateLimitOptions();
        configure?.Invoke(options);

        builder.RequireChatClientBuilder().Use(inner => new RateLimitingChatClient(inner, options));
        return builder;
    }
}
