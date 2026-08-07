using Microsoft.Extensions.AI;

namespace AIBuilder.Rag;

/// <summary>Retrieves the most relevant chunks for a query.</summary>
public interface IRetriever
{
    /// <summary>Embeds <paramref name="query"/> and returns the most similar chunks.</summary>
    Task<IReadOnlyList<ScoredChunk>> RetrieveAsync(string query, int topK = 5, CancellationToken cancellationToken = default);
}

/// <summary>
/// Default retriever that embeds the query and searches an <see cref="IVectorStore"/>.
/// </summary>
public sealed class Retriever : IRetriever
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
    private readonly IVectorStore _vectorStore;

    /// <summary>Initializes a new instance of the <see cref="Retriever"/> class.</summary>
    public Retriever(
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
        IVectorStore vectorStore)
    {
        ArgumentNullException.ThrowIfNull(embeddingGenerator);
        ArgumentNullException.ThrowIfNull(vectorStore);
        _embeddingGenerator = embeddingGenerator;
        _vectorStore = vectorStore;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ScoredChunk>> RetrieveAsync(string query, int topK = 5, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(query);

        GeneratedEmbeddings<Embedding<float>> embeddings =
            await _embeddingGenerator.GenerateAsync([query], cancellationToken: cancellationToken).ConfigureAwait(false);

        return await _vectorStore.SearchAsync(embeddings[0].Vector, topK, cancellationToken).ConfigureAwait(false);
    }
}
