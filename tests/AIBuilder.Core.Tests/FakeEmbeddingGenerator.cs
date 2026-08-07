using Microsoft.Extensions.AI;

namespace AIBuilder.Core.Tests;

/// <summary>
/// A deterministic bag-of-words embedding generator for tests over a fixed vocabulary.
/// </summary>
internal sealed class FakeEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
{
    private static readonly string[] Vocabulary =
        ["cats", "cars", "pets", "fuel", "great", "drive", "are", "need", "space", "finance"];

    public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values,
        EmbeddingGenerationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var embeddings = new GeneratedEmbeddings<Embedding<float>>(
            values.Select(v => new Embedding<float>(Vectorize(v))));

        return Task.FromResult(embeddings);
    }

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose()
    {
    }

    private static float[] Vectorize(string text)
    {
        var vector = new float[Vocabulary.Length];
        foreach (string token in text.ToLowerInvariant().Split(
            [' ', ',', '.', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries))
        {
            int index = Array.IndexOf(Vocabulary, token);
            if (index >= 0)
            {
                vector[index] += 1f;
            }
        }

        return vector;
    }
}
