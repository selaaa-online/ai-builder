using AIBuilder.Rag;
using Xunit;

namespace AIBuilder.Core.Tests;

public class RagTests
{
    [Fact]
    public void FixedSizeChunker_SplitsWithOverlap()
    {
        var chunker = new FixedSizeTextChunker(chunkSize: 10, overlap: 2);
        var document = DocumentLoader.FromText("d1", new string('a', 25));

        List<TextChunk> chunks = [.. chunker.Chunk(document)];

        Assert.True(chunks.Count >= 3);
        Assert.All(chunks, c => Assert.True(c.Text.Length <= 10));
        Assert.Equal(0, chunks[0].Index);
    }

    [Fact]
    public async Task Pipeline_And_Retriever_ReturnMostRelevantChunk()
    {
        var generator = new FakeEmbeddingGenerator();
        var store = new InMemoryVectorStore();
        var pipeline = new RagPipeline(generator, new FixedSizeTextChunker(1000, 0), store);

        await pipeline.IngestAsync(DocumentLoader.FromText("d1", "cats are great pets"));
        await pipeline.IngestAsync(DocumentLoader.FromText("d2", "cars need fuel drive"));

        var retriever = new Retriever(generator, store);
        IReadOnlyList<ScoredChunk> results = await retriever.RetrieveAsync("pets", topK: 1);

        Assert.Single(results);
        Assert.Contains("cats", results[0].Chunk.Text);
        Assert.True(results[0].Score > 0);
    }

    [Fact]
    public async Task VectorStore_Upsert_ReplacesExistingChunk()
    {
        var generator = new FakeEmbeddingGenerator();
        var store = new InMemoryVectorStore();
        var pipeline = new RagPipeline(generator, new FixedSizeTextChunker(1000, 0), store);

        await pipeline.IngestAsync(DocumentLoader.FromText("d1", "cats pets"));
        await pipeline.IngestAsync(DocumentLoader.FromText("d1", "cars fuel"));

        var retriever = new Retriever(generator, store);
        IReadOnlyList<ScoredChunk> results = await retriever.RetrieveAsync("fuel", topK: 5);

        Assert.Single(results);
        Assert.Contains("cars", results[0].Chunk.Text);
    }
}
