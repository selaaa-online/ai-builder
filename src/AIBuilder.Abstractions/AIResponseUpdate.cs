namespace AIBuilder;

/// <summary>
/// An incremental update emitted while streaming an AI response.
/// </summary>
public sealed class AIResponseUpdate
{
    /// <summary>The text fragment for this update.</summary>
    public required string Text { get; init; }
}
