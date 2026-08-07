namespace AIBuilder;

/// <summary>
/// A named, reusable prompt template.
/// </summary>
public sealed class PromptTemplate
{
    /// <summary>The unique template name.</summary>
    public required string Name { get; init; }

    /// <summary>The template body. Variables use <c>{{name}}</c> syntax.</summary>
    public required string Template { get; init; }

    /// <summary>An optional system prompt associated with the template.</summary>
    public string? SystemPrompt { get; init; }
}

/// <summary>
/// A store of named prompt templates.
/// </summary>
public interface IPromptTemplateStore
{
    /// <summary>Gets a template by name, throwing if it does not exist.</summary>
    PromptTemplate Get(string name);

    /// <summary>Attempts to get a template by name.</summary>
    bool TryGet(string name, out PromptTemplate? template);
}
