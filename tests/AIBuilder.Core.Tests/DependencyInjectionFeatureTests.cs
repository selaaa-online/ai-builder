using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AIBuilder.Core.Tests;

public class DependencyInjectionFeatureTests
{
    private sealed class WeatherTools
    {
        public string GetWeather(string city) => $"Sunny in {city}";
    }

    [Fact]
    public void AddTool_Delegate_RegistersNamedTool()
    {
        var services = new ServiceCollection();
        services.AddAIBuilder()
            .UseChatClient(StubChatClient.Returning("x"))
            .AddTool((string city) => $"Sunny in {city}", "get_weather", "Gets the weather");

        using ServiceProvider provider = services.BuildServiceProvider();
        List<AITool> tools = [.. provider.GetServices<AITool>()];

        Assert.Single(tools);
        Assert.Equal("get_weather", tools[0].Name);
    }

    [Fact]
    public void AddTools_Type_RegistersMethodAsTool()
    {
        var services = new ServiceCollection();
        services.AddAIBuilder()
            .UseChatClient(StubChatClient.Returning("x"))
            .AddTools<WeatherTools>();

        using ServiceProvider provider = services.BuildServiceProvider();
        List<AITool> tools = [.. provider.GetServices<AITool>()];

        Assert.Single(tools);
        Assert.Equal(nameof(WeatherTools.GetWeather), tools[0].Name);
    }

    [Fact]
    public async Task UseMemoryCache_ServesSecondIdenticalRequestFromCache()
    {
        var stub = StubChatClient.Returning("cached");

        var services = new ServiceCollection();
        services.AddAIBuilder()
            .UseChatClient(stub)
            .UseMemoryCache();

        using ServiceProvider provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IAIClient>();

        await client.Prompt("same prompt").ExecuteAsync();
        await client.Prompt("same prompt").ExecuteAsync();

        Assert.Equal(1, stub.CallCount);
    }
}
