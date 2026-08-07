using AIBuilder.Middleware;
using Microsoft.Extensions.AI;
using Xunit;

namespace AIBuilder.Core.Tests;

public class RetryChatClientTests
{
    [Fact]
    public async Task GetResponseAsync_RetriesTransientFailures()
    {
        var attempts = 0;
        var stub = new StubChatClient((_, _, _) =>
        {
            attempts++;
            if (attempts < 3)
            {
                throw new HttpRequestException("temporary", null, System.Net.HttpStatusCode.ServiceUnavailable);
            }

            return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, "recovered")));
        });

        var retry = new RetryChatClient(stub, new RetryOptions
        {
            MaxRetries = 5,
            BaseDelay = TimeSpan.FromMilliseconds(1),
        });

        ChatResponse response = await retry.GetResponseAsync([new ChatMessage(ChatRole.User, "hi")]);

        Assert.Equal("recovered", response.Text);
        Assert.Equal(3, attempts);
    }

    [Fact]
    public async Task GetResponseAsync_DoesNotRetryNonTransientFailures()
    {
        var attempts = 0;
        var stub = new StubChatClient((_, _, _) =>
        {
            attempts++;
            throw new InvalidOperationException("permanent");
        });

        var retry = new RetryChatClient(stub, new RetryOptions
        {
            MaxRetries = 5,
            BaseDelay = TimeSpan.FromMilliseconds(1),
        });

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => retry.GetResponseAsync([new ChatMessage(ChatRole.User, "hi")]));

        Assert.Equal(1, attempts);
    }

    [Fact]
    public async Task GetResponseAsync_StopsAfterMaxRetries()
    {
        var attempts = 0;
        var stub = new StubChatClient((_, _, _) =>
        {
            attempts++;
            throw new TimeoutException("always");
        });

        var retry = new RetryChatClient(stub, new RetryOptions
        {
            MaxRetries = 2,
            BaseDelay = TimeSpan.FromMilliseconds(1),
        });

        await Assert.ThrowsAsync<TimeoutException>(
            () => retry.GetResponseAsync([new ChatMessage(ChatRole.User, "hi")]));

        Assert.Equal(3, attempts);
    }
}
