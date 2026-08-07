using AIBuilder;
using AIBuilder.Rag;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Options for configuring the RAG components.
/// </summary>
public sealed class RagOptions
{
    /// <summary>The maximum chunk size in characters.</summary>
    public int ChunkSize { get; set; } = 1000;

    /// <summary>The overlap between consecutive chunks in characters.</summary>
    public int ChunkOverlap { get; set; } = 100;
}

/// <summary>
/// RAG registration extensions.
/// </summary>
public static class AIBuilderRagExtensions
{
    /// <summary>
    /// Registers the in-memory RAG pipeline (chunker, vector store, retriever, pipeline).
    /// An <c>IEmbeddingGenerator&lt;string, Embedding&lt;float&gt;&gt;</c> must be registered separately.
    /// </summary>
    public static IAIBuilder AddRag(this IAIBuilder builder, Action<RagOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Services.AddRag(configure);
        return builder;
    }

    /// <summary>
    /// Registers the in-memory RAG pipeline on a service collection.
    /// </summary>
    public static IServiceCollection AddRag(this IServiceCollection services, Action<RagOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new RagOptions();
        configure?.Invoke(options);

        services.TryAddSingleton<ITextChunker>(new FixedSizeTextChunker(options.ChunkSize, options.ChunkOverlap));
        services.TryAddSingleton<IVectorStore, InMemoryVectorStore>();
        services.TryAddSingleton<IRetriever, Retriever>();
        services.TryAddSingleton<RagPipeline>();

        return services;
    }
}
