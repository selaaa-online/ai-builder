namespace AIBuilder.Middleware;

/// <summary>
/// Per-model pricing, expressed as cost per one million tokens.
/// </summary>
public sealed class ModelPricing
{
    /// <summary>Cost per one million input (prompt) tokens.</summary>
    public required decimal InputPerMillionTokens { get; init; }

    /// <summary>Cost per one million output (completion) tokens.</summary>
    public required decimal OutputPerMillionTokens { get; init; }
}

/// <summary>
/// A computed cost estimate for a single AI request.
/// </summary>
public sealed class CostReport
{
    /// <summary>The model the estimate was computed for.</summary>
    public required string ModelId { get; init; }

    /// <summary>Input (prompt) tokens.</summary>
    public long InputTokens { get; init; }

    /// <summary>Output (completion) tokens.</summary>
    public long OutputTokens { get; init; }

    /// <summary>Estimated cost of the input tokens.</summary>
    public decimal InputCost { get; init; }

    /// <summary>Estimated cost of the output tokens.</summary>
    public decimal OutputCost { get; init; }

    /// <summary>Total estimated cost.</summary>
    public decimal TotalCost => InputCost + OutputCost;

    /// <summary>The ISO currency code for the amounts.</summary>
    public string Currency { get; init; } = "USD";
}

/// <summary>
/// Options controlling cost tracking.
/// </summary>
public sealed class CostOptions
{
    /// <summary>The key used to store the <see cref="CostReport"/> in the response's additional properties.</summary>
    public const string ResponsePropertyKey = "aibuilder.cost";

    /// <summary>Pricing per model identifier.</summary>
    public Dictionary<string, ModelPricing> Pricing { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>The currency code applied to computed reports.</summary>
    public string Currency { get; set; } = "USD";

    /// <summary>An optional callback invoked with each computed report.</summary>
    public Action<CostReport>? OnCostComputed { get; set; }

    /// <summary>Adds pricing for a model.</summary>
    public CostOptions AddModel(string modelId, decimal inputPerMillionTokens, decimal outputPerMillionTokens)
    {
        ArgumentException.ThrowIfNullOrEmpty(modelId);
        Pricing[modelId] = new ModelPricing
        {
            InputPerMillionTokens = inputPerMillionTokens,
            OutputPerMillionTokens = outputPerMillionTokens,
        };
        return this;
    }
}
