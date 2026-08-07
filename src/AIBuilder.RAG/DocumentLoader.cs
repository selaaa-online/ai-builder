using System.Text;

namespace AIBuilder.Rag;

/// <summary>Loads documents from text sources.</summary>
public static class DocumentLoader
{
    /// <summary>Creates a document from raw text.</summary>
    public static RagDocument FromText(string id, string text, IReadOnlyDictionary<string, string>? metadata = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);
        ArgumentNullException.ThrowIfNull(text);
        return new RagDocument { Id = id, Text = text, Metadata = metadata };
    }

    /// <summary>Loads a plain-text or markdown file as a document. The file name is used as the id.</summary>
    public static async Task<RagDocument> FromFileAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        string text = await File.ReadAllTextAsync(path, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
        return new RagDocument
        {
            Id = Path.GetFileName(path),
            Text = text,
            Metadata = new Dictionary<string, string> { ["path"] = path },
        };
    }
}

/// <summary>Formats retrieved chunks into a context block for prompt injection.</summary>
public static class RagContextBuilder
{
    /// <summary>Builds a context string from scored chunks.</summary>
    public static string BuildContext(IEnumerable<ScoredChunk> chunks)
    {
        ArgumentNullException.ThrowIfNull(chunks);

        var builder = new StringBuilder();
        var index = 1;
        foreach (ScoredChunk scored in chunks)
        {
            builder.Append('[').Append(index++).Append("] ");
            builder.AppendLine(scored.Chunk.Text);
            builder.AppendLine();
        }

        return builder.ToString().TrimEnd();
    }
}
