using Microsoft.Extensions.AI;

namespace AIBuilder.Middleware;

/// <summary>
/// A <see cref="DelegatingChatClient"/> that retries transient failures using exponential backoff.
/// </summary>
/// <remarks>
/// Retries apply to <see cref="GetResponseAsync"/> only. Streaming responses are passed through
/// without retry, because a stream may already have yielded partial content before failing.
/// </remarks>
public sealed class RetryChatClient : DelegatingChatClient
{
    private readonly RetryOptions _options;

    /// <summary>Initializes a new instance of the <see cref="RetryChatClient"/> class.</summary>
    /// <param name="innerClient">The inner chat client to wrap.</param>
    /// <param name="options">The retry options.</param>
    public RetryChatClient(IChatClient innerClient, RetryOptions options)
        : base(innerClient)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
    }

    /// <inheritdoc />
    public override async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var attempt = 0;
        while (true)
        {
            try
            {
                return await base.GetResponseAsync(messages, options, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (attempt < _options.MaxRetries && ShouldRetry(ex))
            {
                TimeSpan delay = ComputeDelay(attempt);
                attempt++;
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private bool ShouldRetry(Exception ex)
    {
        if (ex is OperationCanceledException)
        {
            return false;
        }

        return _options.ShouldRetry?.Invoke(ex) ?? IsTransient(ex);
    }

    private TimeSpan ComputeDelay(int attempt)
    {
        double milliseconds = _options.BaseDelay.TotalMilliseconds * Math.Pow(2, attempt);
        double jitter = Random.Shared.NextDouble() * _options.BaseDelay.TotalMilliseconds;
        double capped = Math.Min(milliseconds + jitter, _options.MaxDelay.TotalMilliseconds);
        return TimeSpan.FromMilliseconds(capped);
    }

    private static bool IsTransient(Exception ex)
    {
        return ex switch
        {
            HttpRequestException httpEx => httpEx.StatusCode is null || IsTransientStatus((int)httpEx.StatusCode.Value),
            System.ClientModel.ClientResultException clientEx => clientEx.Status == 0 || IsTransientStatus(clientEx.Status),
            TimeoutException => true,
            IOException => true,
            _ => false,
        };
    }

    private static bool IsTransientStatus(int status) =>
        status == 408 || status == 429 || status >= 500;
}
