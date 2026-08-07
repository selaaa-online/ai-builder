using AIBuilder;
using Microsoft.Extensions.AI;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// OpenTelemetry registration extensions for <see cref="IAIBuilder"/>.
/// </summary>
public static class AIBuilderTelemetryExtensions
{
    /// <summary>The default OpenTelemetry activity/meter source name emitted by AIBuilder.</summary>
    public const string DefaultSourceName = "AIBuilder";

    /// <summary>
    /// Adds OpenTelemetry instrumentation (traces and metrics) to the pipeline.
    /// Register the source name with your OpenTelemetry provider to export the data.
    /// </summary>
    /// <param name="builder">The AIBuilder instance.</param>
    /// <param name="sourceName">The activity/meter source name. Defaults to <c>AIBuilder</c>.</param>
    /// <param name="configure">An optional delegate to configure the telemetry client.</param>
    public static IAIBuilder UseTelemetry(
        this IAIBuilder builder,
        string sourceName = DefaultSourceName,
        Action<OpenTelemetryChatClient>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrEmpty(sourceName);

        builder.RequireChatClientBuilder().UseOpenTelemetry(sourceName: sourceName, configure: configure);
        return builder;
    }
}
