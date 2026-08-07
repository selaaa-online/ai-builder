using Xunit;

namespace AIBuilder.IntegrationTests;

/// <summary>
/// A <see cref="FactAttribute"/> that skips the test unless an <c>OPENAI_API_KEY</c>
/// environment variable is present.
/// </summary>
public sealed class OpenAIFactAttribute : FactAttribute
{
    public OpenAIFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("OPENAI_API_KEY")))
        {
            Skip = "Set OPENAI_API_KEY to run OpenAI integration tests.";
        }
    }
}
