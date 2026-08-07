using AIBuilder.Middleware;
using Microsoft.Extensions.AI;
using Xunit;

namespace AIBuilder.Core.Tests;

public class CostChatClientTests
{
    [Fact]
    public async Task GetResponseAsync_AttachesCostReport()
    {
        var stub = new StubChatClient((_, _, _) =>
        {
            var response = new ChatResponse(new ChatMessage(ChatRole.Assistant, "hi"))
            {
                ModelId = "gpt-4o-mini",
                Usage = new UsageDetails { InputTokenCount = 1_000, OutputTokenCount = 2_000 },
            };
            return Task.FromResult(response);
        });

        CostReport? captured = null;
        var options = new CostOptions { OnCostComputed = r => captured = r };
        options.AddModel("gpt-4o-mini", inputPerMillionTokens: 0.15m, outputPerMillionTokens: 0.60m);

        var cost = new CostChatClient(stub, options);
        ChatResponse response = await cost.GetResponseAsync([new ChatMessage(ChatRole.User, "hi")]);

        Assert.NotNull(captured);
        Assert.Equal(1_000, captured!.InputTokens);
        Assert.Equal(2_000, captured.OutputTokens);
        Assert.Equal(0.15m * 1_000 / 1_000_000 + 0.60m * 2_000 / 1_000_000, captured.TotalCost);
        Assert.True(response.AdditionalProperties!.ContainsKey(CostOptions.ResponsePropertyKey));
    }

    [Fact]
    public async Task GetResponseAsync_NoPricing_DoesNotAttachReport()
    {
        var stub = new StubChatClient((_, _, _) => Task.FromResult(
            new ChatResponse(new ChatMessage(ChatRole.Assistant, "hi"))
            {
                ModelId = "unknown-model",
                Usage = new UsageDetails { InputTokenCount = 10, OutputTokenCount = 10 },
            }));

        var cost = new CostChatClient(stub, new CostOptions());
        ChatResponse response = await cost.GetResponseAsync([new ChatMessage(ChatRole.User, "hi")]);

        Assert.True(response.AdditionalProperties is null || !response.AdditionalProperties.ContainsKey(CostOptions.ResponsePropertyKey));
    }

    [Fact]
    public async Task GetResponseAsync_MatchesDatedModelIdByPrefix()
    {
        var stub = new StubChatClient((_, _, _) => Task.FromResult(
            new ChatResponse(new ChatMessage(ChatRole.Assistant, "hi"))
            {
                ModelId = "gpt-4o-mini-2024-07-18",
                Usage = new UsageDetails { InputTokenCount = 1_000_000, OutputTokenCount = 0 },
            }));

        var options = new CostOptions();
        options.AddModel("gpt-4o-mini", inputPerMillionTokens: 0.15m, outputPerMillionTokens: 0.60m);

        var cost = new CostChatClient(stub, options);
        ChatResponse response = await cost.GetResponseAsync([new ChatMessage(ChatRole.User, "hi")]);

        var report = (CostReport)response.AdditionalProperties![CostOptions.ResponsePropertyKey]!;
        Assert.Equal(0.15m, report.TotalCost);
    }
}
