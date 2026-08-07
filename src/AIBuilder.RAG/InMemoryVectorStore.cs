namespace AIBuilder.Rag;

/// <summary>Stores embedded chunks and performs similarity search.</summary>
public interface IVectorStore
{
    /// <summary>Adds or updates embedded chunks.</summary>
    Task UpsertAsync(IEnumerable<EmbeddedChunk> chunks, CancellationToken cancellationToken = default);

    /// <summary>Returns the <paramref name="topK"/> chunks most similar to <paramref name="query"/>.</summary>
    Task<IReadOnlyList<ScoredChunk>> SearchAsync(ReadOnlyMemory<float> query, int topK, CancellationToken cancellationToken = default);
}

/// <summary>
/// A thread-safe, in-memory vector store using cosine similarity.
/// </summary>
public sealed class InMemoryVectorStore : IVectorStore
{
    private readonly List<EmbeddedChunk> _chunks = [];
    private readonly object _sync = new();

    /// <inheritdoc />
    public Task UpsertAsync(IEnumerable<EmbeddedChunk> chunks, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chunks);

        lock (_sync)
        {
            foreach (EmbeddedChunk chunk in chunks)
            {
                _chunks.RemoveAll(c => c.Chunk.DocumentId == chunk.Chunk.DocumentId && c.Chunk.Index == chunk.Chunk.Index);
                _chunks.Add(chunk);
            }
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<ScoredChunk>> SearchAsync(ReadOnlyMemory<float> query, int topK, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(topK);

        EmbeddedChunk[] snapshot;
        lock (_sync)
        {
            snapshot = [.. _chunks];
        }

        IReadOnlyList<ScoredChunk> results = snapshot
            .Select(c => new ScoredChunk
            {
                Chunk = c.Chunk,
                Score = VectorMath.CosineSimilarity(query.Span, c.Embedding.Span),
            })
            .OrderByDescending(s => s.Score)
            .Take(topK)
            .ToList();

        return Task.FromResult(results);
    }
}
