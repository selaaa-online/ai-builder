namespace AIBuilder.Prompts;

/// <summary>
/// A simple in-memory <see cref="IPromptTemplateStore"/>.
/// </summary>
public sealed class InMemoryPromptTemplateStore : IPromptTemplateStore
{
    private readonly Dictionary<string, PromptTemplate> _templates;

    /// <summary>Initializes a new instance of the <see cref="InMemoryPromptTemplateStore"/> class.</summary>
    public InMemoryPromptTemplateStore(IEnumerable<PromptTemplate> templates)
    {
        ArgumentNullException.ThrowIfNull(templates);
        _templates = templates.ToDictionary(t => t.Name, StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public PromptTemplate Get(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        return _templates.TryGetValue(name, out PromptTemplate? template)
            ? template
            : throw new KeyNotFoundException($"No prompt template named '{name}' is registered.");
    }

    /// <inheritdoc />
    public bool TryGet(string name, out PromptTemplate? template)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        return _templates.TryGetValue(name, out template);
    }
}
