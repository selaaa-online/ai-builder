using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AIBuilder.Core.Tests;

public class ServiceCollectionTests
{
    [Fact]
    public async Task AddAIBuilder_WithProviderAndMiddleware_ResolvesWorkingClient()
    {
        var services = new ServiceCollection();

        services.AddAIBuilder()
            .UseChatClient(StubChatClient.Returning("wired"))
            .UseRetry(o => o.BaseDelay = TimeSpan.FromMilliseconds(1));

        using ServiceProvider provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IAIClient>();

        AIResponse response = await client.Prompt("hi").ExecuteAsync();

        Assert.Equal("wired", response.Text);
    }

    [Fact]
    public void UseRetry_WithoutProvider_Throws()
    {
        var services = new ServiceCollection();
        IAIBuilder builder = services.AddAIBuilder();

        Assert.Throws<InvalidOperationException>(() => builder.UseRetry());
    }
}
