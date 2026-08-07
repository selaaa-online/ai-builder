namespace AIBuilder.Rag;

/// <summary>Splits documents into chunks suitable for embedding.</summary>
public interface ITextChunker
{
    /// <summary>Splits a document into chunks.</summary>
    IEnumerable<TextChunk> Chunk(RagDocument document);
}

/// <summary>
/// Splits text into fixed-size chunks (by character count) with a configurable overlap.
/// </summary>
public sealed class FixedSizeTextChunker : ITextChunker
{
    private readonly int _chunkSize;
    private readonly int _overlap;

    /// <summary>Initializes a new instance of the <see cref="FixedSizeTextChunker"/> class.</summary>
    /// <param name="chunkSize">The maximum chunk size in characters.</param>
    /// <param name="overlap">The number of overlapping characters between consecutive chunks.</param>
    public FixedSizeTextChunker(int chunkSize = 1000, int overlap = 100)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(chunkSize);

        if (overlap < 0 || overlap >= chunkSize)
        {
            throw new ArgumentOutOfRangeException(nameof(overlap), "Overlap must be non-negative and smaller than the chunk size.");
        }

        _chunkSize = chunkSize;
        _overlap = overlap;
    }

    /// <inheritdoc />
    public IEnumerable<TextChunk> Chunk(RagDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        string text = document.Text;
        if (string.IsNullOrEmpty(text))
        {
            yield break;
        }

        int step = _chunkSize - _overlap;
        int index = 0;

        for (int start = 0; start < text.Length; start += step)
        {
            int length = Math.Min(_chunkSize, text.Length - start);
            yield return new TextChunk
            {
                DocumentId = document.Id,
                Index = index++,
                Text = text.Substring(start, length),
                Metadata = document.Metadata,
            };

            if (start + length >= text.Length)
            {
                break;
            }
        }
    }
}
