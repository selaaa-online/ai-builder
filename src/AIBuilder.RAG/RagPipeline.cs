using Microsoft.Extensions.AI;

namespace AIBuilder.Rag;

/// <summary>
/// Ingests documents by chunking, embedding, and storing them in a vector store.
/// </summary>
public sealed class RagPipeline
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
    private readonly ITextChunker _chunker;
    private readonly IVectorStore _vectorStore;

    /// <summary>Initializes a new instance of the <see cref="RagPipeline"/> class.</summary>
    public RagPipeline(
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
        ITextChunker chunker,
        IVectorStore vectorStore)
    {
        ArgumentNullException.ThrowIfNull(embeddingGenerator);
        ArgumentNullException.ThrowIfNull(chunker);
        ArgumentNullException.ThrowIfNull(vectorStore);
        _embeddingGenerator = embeddingGenerator;
        _chunker = chunker;
        _vectorStore = vectorStore;
    }

    /// <summary>Chunks, embeds, and stores a document.</summary>
    public async Task IngestAsync(RagDocument document, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);

        List<TextChunk> chunks = [.. _chunker.Chunk(document)];
        if (chunks.Count == 0)
        {
            return;
        }

        GeneratedEmbeddings<Embedding<float>> embeddings = await _embeddingGenerator
            .GenerateAsync(chunks.Select(c => c.Text), cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var embedded = new List<EmbeddedChunk>(chunks.Count);
        for (int i = 0; i < chunks.Count; i++)
        {
            embedded.Add(new EmbeddedChunk { Chunk = chunks[i], Embedding = embeddings[i].Vector });
        }

        await _vectorStore.UpsertAsync(embedded, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Ingests multiple documents.</summary>
    public async Task IngestAsync(IEnumerable<RagDocument> documents, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(documents);

        foreach (RagDocument document in documents)
        {
            await IngestAsync(document, cancellationToken).ConfigureAwait(false);
        }
    }
}
