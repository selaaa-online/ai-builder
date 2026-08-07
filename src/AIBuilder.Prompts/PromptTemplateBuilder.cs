namespace AIBuilder.Prompts;

/// <summary>
/// Collects prompt templates for registration.
/// </summary>
public sealed class PromptTemplateBuilder
{
    private readonly List<PromptTemplate> _templates = [];

    /// <summary>The templates collected so far.</summary>
    public IReadOnlyList<PromptTemplate> Templates => _templates;

    /// <summary>Adds a template.</summary>
    /// <param name="name">The unique template name.</param>
    /// <param name="template">The template body using <c>{{name}}</c> variables.</param>
    /// <param name="systemPrompt">An optional system prompt.</param>
    public PromptTemplateBuilder Add(string name, string template, string? systemPrompt = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(template);

        _templates.Add(new PromptTemplate
        {
            Name = name,
            Template = template,
            SystemPrompt = systemPrompt,
        });

        return this;
    }
}
