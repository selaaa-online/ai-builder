using AIBuilder.Memory;
using AIBuilder.Prompts;
using Microsoft.Extensions.AI;
using Xunit;

namespace AIBuilder.Core.Tests;

public class FeatureIntegrationTests
{
    [Fact]
    public async Task Template_RendersVariablesAndSystemPrompt()
    {
        var store = new InMemoryPromptTemplateStore(
        [
            new PromptTemplate { Name = "greet", Template = "Hi {{name}}", SystemPrompt = "Be nice." },
        ]);

        List<ChatMessage>? captured = null;
        var stub = new StubChatClient((messages, _, _) =>
        {
            captured = messages.ToList();
            return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, "ok")));
        });

        var client = new AIClient(stub, templateStore: store);
        await client.Template("greet").With("name", "Ada").ExecuteAsync();

        Assert.NotNull(captured);
        Assert.Equal("Be nice.", captured![0].Text);
        Assert.Equal(ChatRole.System, captured[0].Role);
        Assert.Equal("Hi Ada", captured[^1].Text);
    }

    [Fact]
    public async Task Chat_PersistsAndReloadsConversation()
    {
        var conversations = new InMemoryConversationStore();

        List<ChatMessage>? lastMessages = null;
        var counter = 0;
        var stub = new StubChatClient((messages, _, _) =>
        {
            lastMessages = messages.ToList();
            counter++;
            return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, $"answer{counter}")));
        });

        var client = new AIClient(stub, conversationStore: conversations);

        await client.Prompt("first").Chat("c1").ExecuteAsync();

        IReadOnlyList<ConversationMessage> history = await conversations.GetAsync("c1");
        Assert.Equal(2, history.Count);
        Assert.Equal("first", history[0].Text);
        Assert.Equal("answer1", history[1].Text);

        await client.Prompt("second").Chat("c1").ExecuteAsync();

        Assert.NotNull(lastMessages);
        Assert.Equal("first", lastMessages![0].Text);
        Assert.Equal("answer1", lastMessages[1].Text);
        Assert.Equal("second", lastMessages[2].Text);
    }
}
