using System.Text;

namespace AIBuilder;

/// <summary>
/// Renders prompt templates that use <c>{{name}}</c> placeholder syntax.
/// </summary>
public static class PromptTemplateRenderer
{
    /// <summary>
    /// Replaces <c>{{name}}</c> placeholders in <paramref name="template"/> with the supplied values.
    /// </summary>
    /// <param name="template">The template text.</param>
    /// <param name="variables">The variable values keyed by placeholder name.</param>
    /// <returns>The rendered text.</returns>
    public static string Render(string template, IReadOnlyDictionary<string, object?> variables)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(variables);

        var result = new StringBuilder(template.Length);
        var index = 0;

        while (index < template.Length)
        {
            int open = template.IndexOf("{{", index, StringComparison.Ordinal);
            if (open < 0)
            {
                result.Append(template, index, template.Length - index);
                break;
            }

            int close = template.IndexOf("}}", open + 2, StringComparison.Ordinal);
            if (close < 0)
            {
                result.Append(template, index, template.Length - index);
                break;
            }

            result.Append(template, index, open - index);

            string key = template.Substring(open + 2, close - (open + 2)).Trim();
            if (variables.TryGetValue(key, out object? value))
            {
                result.Append(value?.ToString() ?? string.Empty);
            }
            else
            {
                result.Append(template, open, close + 2 - open);
            }

            index = close + 2;
        }

        return result.ToString();
    }
}
