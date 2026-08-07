using AIBuilder;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AIBuilder.IntegrationTests;

/// <summary>
/// Live OpenAI tests. Skipped unless OPENAI_API_KEY is set.
/// </summary>
public class OpenAIPipelineTests
{
    private static ServiceProvider BuildProvider()
    {
        string apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")!;
        string model = Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? "gpt-4o-mini";

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAIBuilder()
            .UseOpenAI(apiKey, model)
            .UseLogging()
            .UseRetry()
            .UseMemoryCache()
            .UseCost(o => o.AddModel(model, inputPerMillionTokens: 0.15m, outputPerMillionTokens: 0.60m));

        return services.BuildServiceProvider();
    }

    [OpenAIFact]
    public async Task Prompt_ReturnsNonEmptyText()
    {
        using ServiceProvider provider = BuildProvider();
        var ai = provider.GetRequiredService<IAIClient>();

        AIResponse response = await ai
            .Prompt("Reply with exactly one word: pong")
            .Temperature(0f)
            .ExecuteAsync();

        Assert.False(string.IsNullOrWhiteSpace(response.Text));
    }

    [OpenAIFact]
    public async Task Cache_ServesIdenticalRequestFromCache()
    {
        using ServiceProvider provider = BuildProvider();
        var ai = provider.GetRequiredService<IAIClient>();

        AIResponse first = await ai.Prompt("Reply with exactly one word: cached").Temperature(0f).ExecuteAsync();
        AIResponse second = await ai.Prompt("Reply with exactly one word: cached").Temperature(0f).ExecuteAsync();

        Assert.Equal(first.Text, second.Text);
    }

    private sealed class Person
    {
        public string? Name { get; set; }

        public int Age { get; set; }
    }

    [OpenAIFact]
    public async Task StructuredOutput_Deserializes()
    {
        using ServiceProvider provider = BuildProvider();
        var ai = provider.GetRequiredService<IAIClient>();

        AIResponse<Person> response = await ai
            .Prompt("Extract the person: John Doe is 34 years old.")
            .As<Person>()
            .ExecuteAsync();

        Assert.True(response.IsValid);
        Assert.Equal("John Doe", response.Value?.Name);
        Assert.Equal(34, response.Value?.Age);
    }
}
