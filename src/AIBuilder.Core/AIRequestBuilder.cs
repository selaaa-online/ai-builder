using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;

namespace AIBuilder;

internal sealed class AIRequestBuilder : IAIRequestBuilder
{
    private readonly AIRequestState _state;

    public AIRequestBuilder(AIRequestState state) => _state = state;

    public IAIRequestBuilder SystemPrompt(string systemPrompt)
    {
        _state.SystemPrompt = systemPrompt;
        return this;
    }

    public IAIRequestBuilder WithContext(string context)
    {
        _state.ContextText = context;
        return this;
    }

    public IAIRequestBuilder Temperature(float temperature)
    {
        _state.Options.Temperature = temperature;
        return this;
    }

    public IAIRequestBuilder MaxTokens(int maxTokens)
    {
        _state.Options.MaxOutputTokens = maxTokens;
        return this;
    }

    public IAIRequestBuilder Model(string modelId)
    {
        _state.Options.ModelId = modelId;
        return this;
    }

    public IAIRequestBuilder With(string key, object? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        _state.Variables[key] = value;
        return this;
    }

    public IAIRequestBuilder Chat(string conversationId)
    {
        ArgumentException.ThrowIfNullOrEmpty(conversationId);
        _state.ConversationId = conversationId;
        return this;
    }

    public IAIRequestBuilder WithTools(params AITool[] tools)
    {
        if (tools is null || tools.Length == 0)
        {
            _state.UseRegisteredTools = true;
        }
        else
        {
            _state.Tools.AddRange(tools);
        }

        return this;
    }

    public IAIRequestBuilder<T> As<T>() => new AIRequestBuilder<T>(_state);

    public async Task<AIResponse> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        AIPreparedRequest prepared = await AIRequestExecutor.PrepareAsync(_state, cancellationToken).ConfigureAwait(false);

        ChatResponse response = await _state.Context.ChatClient
            .GetResponseAsync(prepared.Messages, prepared.Options, cancellationToken)
            .ConfigureAwait(false);

        await AIRequestExecutor.PersistAsync(_state, prepared.UserText, response.Text, cancellationToken)
            .ConfigureAwait(false);

        return new AIResponse
        {
            Text = response.Text,
            Usage = response.Usage,
            ModelId = response.ModelId,
            FinishReason = response.FinishReason,
            RawResponse = response,
        };
    }

    public async IAsyncEnumerable<AIResponseUpdate> StreamAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        AIPreparedRequest prepared = await AIRequestExecutor.PrepareAsync(_state, cancellationToken).ConfigureAwait(false);

        var accumulated = new System.Text.StringBuilder();

        IAsyncEnumerable<ChatResponseUpdate> updates = _state.Context.ChatClient
            .GetStreamingResponseAsync(prepared.Messages, prepared.Options, cancellationToken);

        await foreach (ChatResponseUpdate update in updates.ConfigureAwait(false))
        {
            if (!string.IsNullOrEmpty(update.Text))
            {
                accumulated.Append(update.Text);
                yield return new AIResponseUpdate { Text = update.Text };
            }
        }

        await AIRequestExecutor.PersistAsync(_state, prepared.UserText, accumulated.ToString(), cancellationToken)
            .ConfigureAwait(false);
    }
}
