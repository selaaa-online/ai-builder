namespace AIBuilder.Middleware;

/// <summary>
/// Options controlling the retry middleware behavior.
/// </summary>
public sealed class RetryOptions
{
    /// <summary>The maximum number of retry attempts. Default is 3.</summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>The base delay used for exponential backoff. Default is 500ms.</summary>
    public TimeSpan BaseDelay { get; set; } = TimeSpan.FromMilliseconds(500);

    /// <summary>The maximum delay between attempts. Default is 30s.</summary>
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// An optional predicate to determine whether an exception is transient and should be retried.
    /// When <see langword="null"/>, a built-in heuristic is used.
    /// </summary>
    public Func<Exception, bool>? ShouldRetry { get; set; }
}
