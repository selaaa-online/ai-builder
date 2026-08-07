using Microsoft.Extensions.AI;
using Xunit;

namespace AIBuilder.Core.Tests;

public class AIClientTests
{
    [Fact]
    public async Task ExecuteAsync_ReturnsProviderText()
    {
        var stub = StubChatClient.Returning("hello world");
        IAIClient client = new AIClient(stub);

        AIResponse response = await client.Prompt("hi").ExecuteAsync();

        Assert.Equal("hello world", response.Text);
        Assert.Equal(1, stub.CallCount);
    }

    [Fact]
    public async Task ExecuteAsync_IncludesSystemAndContextMessages()
    {
        List<ChatMessage>? captured = null;
        var stub = new StubChatClient((messages, _, _) =>
        {
            captured = messages.ToList();
            return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, "ok")));
        });
        IAIClient client = new AIClient(stub);

        await client.Prompt("prompt")
            .SystemPrompt("system")
            .WithContext("context")
            .ExecuteAsync();

        Assert.NotNull(captured);
        Assert.Equal(3, captured!.Count);
        Assert.Equal(ChatRole.System, captured[0].Role);
        Assert.Equal("system", captured[0].Text);
        Assert.Equal("context", captured[1].Text);
        Assert.Equal("prompt", captured[2].Text);
    }

    [Fact]
    public async Task StreamAsync_YieldsUpdates()
    {
        var stub = StubChatClient.Returning("streamed");
        IAIClient client = new AIClient(stub);

        var chunks = new List<string>();
        await foreach (AIResponseUpdate update in client.Prompt("hi").StreamAsync())
        {
            chunks.Add(update.Text);
        }

        Assert.Contains("streamed", chunks);
    }
}
