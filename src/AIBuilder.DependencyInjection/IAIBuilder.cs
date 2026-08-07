using Microsoft.Extensions.DependencyInjection;

namespace AIBuilder;

/// <summary>
/// Builder used to configure the AIBuilder pipeline and its middleware.
/// </summary>
public interface IAIBuilder
{
    /// <summary>The underlying service collection.</summary>
    IServiceCollection Services { get; }
}
