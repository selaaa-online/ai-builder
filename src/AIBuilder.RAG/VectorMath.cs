namespace AIBuilder.Rag;

/// <summary>Vector math helpers.</summary>
internal static class VectorMath
{
    /// <summary>Computes the cosine similarity between two vectors.</summary>
    public static double CosineSimilarity(ReadOnlySpan<float> a, ReadOnlySpan<float> b)
    {
        if (a.Length != b.Length || a.Length == 0)
        {
            return 0d;
        }

        double dot = 0d;
        double magA = 0d;
        double magB = 0d;

        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            magA += a[i] * a[i];
            magB += b[i] * b[i];
        }

        if (magA == 0d || magB == 0d)
        {
            return 0d;
        }

        return dot / (Math.Sqrt(magA) * Math.Sqrt(magB));
    }
}
