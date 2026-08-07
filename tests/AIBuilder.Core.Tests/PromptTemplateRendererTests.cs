using Xunit;

namespace AIBuilder.Core.Tests;

public class PromptTemplateRendererTests
{
    [Fact]
    public void Render_SubstitutesVariables()
    {
        var vars = new Dictionary<string, object?> { ["name"] = "Ada", ["count"] = 3 };
        string result = PromptTemplateRenderer.Render("Hello {{name}}, you have {{count}} messages.", vars);
        Assert.Equal("Hello Ada, you have 3 messages.", result);
    }

    [Fact]
    public void Render_LeavesUnknownPlaceholders()
    {
        var vars = new Dictionary<string, object?>();
        string result = PromptTemplateRenderer.Render("Value: {{missing}}", vars);
        Assert.Equal("Value: {{missing}}", result);
    }

    [Fact]
    public void Render_TrimsPlaceholderWhitespace()
    {
        var vars = new Dictionary<string, object?> { ["x"] = "y" };
        string result = PromptTemplateRenderer.Render("{{ x }}", vars);
        Assert.Equal("y", result);
    }
}
