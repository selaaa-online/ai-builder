namespace AIBuilder.Middleware;

/// <summary>
/// Options controlling the rate-limiting middleware.
/// </summary>
public sealed class RateLimitOptions
{
    /// <summary>The maximum number of requests permitted within <see cref="Window"/>.</summary>
    public int PermitLimit { get; set; } = 60;

    /// <summary>The sliding window duration. Default is one minute.</summary>
    public TimeSpan Window { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// The maximum time a request may wait for a permit before failing.
    /// Default is one minute.
    /// </summary>
    public TimeSpan MaxWait { get; set; } = TimeSpan.FromMinutes(1);
}
