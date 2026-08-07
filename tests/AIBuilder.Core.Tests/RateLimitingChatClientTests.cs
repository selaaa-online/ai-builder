using System.Diagnostics;
using AIBuilder.Middleware;
using Microsoft.Extensions.AI;
using Xunit;

namespace AIBuilder.Core.Tests;

public class RateLimitingChatClientTests
{
    [Fact]
    public async Task GetResponseAsync_AllowsUpToPermitLimitImmediately()
    {
        var stub = StubChatClient.Returning("ok");
        using var limiter = new RateLimitingChatClient(stub, new RateLimitOptions
        {
            PermitLimit = 3,
            Window = TimeSpan.FromSeconds(10),
        });

        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 3; i++)
        {
            await limiter.GetResponseAsync([new ChatMessage(ChatRole.User, "hi")]);
        }

        sw.Stop();
        Assert.True(sw.Elapsed < TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task GetResponseAsync_ThrowsWhenExceededAndMaxWaitElapsed()
    {
        var stub = StubChatClient.Returning("ok");
        using var limiter = new RateLimitingChatClient(stub, new RateLimitOptions
        {
            PermitLimit = 1,
            Window = TimeSpan.FromMinutes(5),
            MaxWait = TimeSpan.Zero,
        });

        await limiter.GetResponseAsync([new ChatMessage(ChatRole.User, "hi")]);

        await Assert.ThrowsAsync<RateLimitExceededException>(
            () => limiter.GetResponseAsync([new ChatMessage(ChatRole.User, "hi")]));
    }
}
