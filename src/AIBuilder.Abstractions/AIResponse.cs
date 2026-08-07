using Microsoft.Extensions.AI;

namespace AIBuilder;

/// <summary>
/// The result of an AI request.
/// </summary>
public class AIResponse
{
    /// <summary>The generated text.</summary>
    public required string Text { get; init; }

    /// <summary>Token usage details, when reported by the provider.</summary>
    public UsageDetails? Usage { get; init; }

    /// <summary>The model that produced the response, when reported by the provider.</summary>
    public string? ModelId { get; init; }

    /// <summary>The reason the model stopped generating, when reported by the provider.</summary>
    public ChatFinishReason? FinishReason { get; init; }

    /// <summary>The underlying <see cref="ChatResponse"/> for advanced scenarios.</summary>
    public ChatResponse? RawResponse { get; init; }
}

/// <summary>
/// The result of a strongly-typed AI request.
/// </summary>
/// <typeparam name="T">The deserialized response type.</typeparam>
public sealed class AIResponse<T> : AIResponse
{
    /// <summary>The deserialized value, or <see langword="null"/> if deserialization failed.</summary>
    public T? Value { get; init; }

    /// <summary>Whether the response was successfully deserialized into <typeparamref name="T"/>.</summary>
    public bool IsValid { get; init; }
}
