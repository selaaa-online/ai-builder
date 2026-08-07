using Microsoft.Extensions.AI;

namespace AIBuilder.Middleware;

/// <summary>
/// A <see cref="DelegatingChatClient"/> that estimates the cost of each request from
/// reported token usage and attaches a <see cref="CostReport"/> to the response.
/// </summary>
public sealed class CostChatClient : DelegatingChatClient
{
    private readonly CostOptions _options;

    /// <summary>Initializes a new instance of the <see cref="CostChatClient"/> class.</summary>
    public CostChatClient(IChatClient innerClient, CostOptions options)
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
        ChatResponse response = await base.GetResponseAsync(messages, options, cancellationToken).ConfigureAwait(false);

        CostReport? report = ComputeReport(response, options);
        if (report is not null)
        {
            response.AdditionalProperties ??= [];
            response.AdditionalProperties[CostOptions.ResponsePropertyKey] = report;
            _options.OnCostComputed?.Invoke(report);
        }

        return response;
    }

    private CostReport? ComputeReport(ChatResponse response, ChatOptions? options)
    {
        if (response.Usage is not { } usage)
        {
            return null;
        }

        string? modelId = response.ModelId ?? options?.ModelId;
        if (modelId is null || !TryGetPricing(modelId, out ModelPricing? pricing))
        {
            return null;
        }

        long inputTokens = usage.InputTokenCount ?? 0;
        long outputTokens = usage.OutputTokenCount ?? 0;

        return new CostReport
        {
            ModelId = modelId,
            InputTokens = inputTokens,
            OutputTokens = outputTokens,
            InputCost = inputTokens / 1_000_000m * pricing.InputPerMillionTokens,
            OutputCost = outputTokens / 1_000_000m * pricing.OutputPerMillionTokens,
            Currency = _options.Currency,
        };
    }

    // Providers often return a dated model id (e.g. "gpt-4o-mini-2024-07-18"); fall back to the
    // longest configured pricing key that is a prefix of the returned model id.
    private bool TryGetPricing(string modelId, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out ModelPricing? pricing)
    {
        if (_options.Pricing.TryGetValue(modelId, out pricing))
        {
            return true;
        }

        pricing = _options.Pricing
            .Where(kv => modelId.StartsWith(kv.Key, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(kv => kv.Key.Length)
            .Select(kv => kv.Value)
            .FirstOrDefault();

        return pricing is not null;
    }
}
