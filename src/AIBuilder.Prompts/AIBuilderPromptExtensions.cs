using AIBuilder;
using AIBuilder.Prompts;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Prompt template registration extensions for <see cref="IAIBuilder"/>.
/// </summary>
public static class AIBuilderPromptExtensions
{
    /// <summary>
    /// Registers reusable prompt templates. Use them with <c>ai.Template("name").With("key", value)</c>.
    /// </summary>
    public static IAIBuilder AddPromptTemplates(this IAIBuilder builder, Action<PromptTemplateBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configure);

        var templateBuilder = new PromptTemplateBuilder();
        configure(templateBuilder);

        builder.Services.TryAddSingleton<IPromptTemplateStore>(
            new InMemoryPromptTemplateStore(templateBuilder.Templates));

        return builder;
    }
}
