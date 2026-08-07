namespace AIBuilder.Rag;

/// <summary>A source document to be ingested into a vector store.</summary>
public sealed class RagDocument
{
    /// <summary>A unique document identifier.</summary>
    public required string Id { get; init; }

    /// <summary>The document text.</summary>
    public required string Text { get; init; }

    /// <summary>Optional metadata carried through to chunks.</summary>
    public IReadOnlyDictionary<string, string>? Metadata { get; init; }
}

/// <summary>A contiguous slice of a document.</summary>
public sealed class TextChunk
{
    /// <summary>The identifier of the source document.</summary>
    public required string DocumentId { get; init; }

    /// <summary>The zero-based index of the chunk within its document.</summary>
    public required int Index { get; init; }

    /// <summary>The chunk text.</summary>
    public required string Text { get; init; }

    /// <summary>Optional metadata inherited from the document.</summary>
    public IReadOnlyDictionary<string, string>? Metadata { get; init; }
}

/// <summary>A chunk together with its embedding vector.</summary>
public sealed class EmbeddedChunk
{
    /// <summary>The chunk.</summary>
    public required TextChunk Chunk { get; init; }

    /// <summary>The embedding vector for the chunk.</summary>
    public required ReadOnlyMemory<float> Embedding { get; init; }
}

/// <summary>A chunk with a relevance score from a similarity search.</summary>
public sealed class ScoredChunk
{
    /// <summary>The matching chunk.</summary>
    public required TextChunk Chunk { get; init; }

    /// <summary>The similarity score (higher is more relevant).</summary>
    public required double Score { get; init; }
}
