using Microsoft.Extensions.AI;

namespace AIBuilder;

internal sealed class AIRequestBuilder<T> : IAIRequestBuilder<T>
{
    private readonly AIRequestState _state;

    public AIRequestBuilder(AIRequestState state) => _state = state;

    public IAIRequestBuilder<T> SystemPrompt(string systemPrompt)
    {
        _state.SystemPrompt = systemPrompt;
        return this;
    }

    public IAIRequestBuilder<T> WithContext(string context)
    {
        _state.ContextText = context;
        return this;
    }

    public IAIRequestBuilder<T> Temperature(float temperature)
    {
        _state.Options.Temperature = temperature;
        return this;
    }

    public IAIRequestBuilder<T> MaxTokens(int maxTokens)
    {
        _state.Options.MaxOutputTokens = maxTokens;
        return this;
    }

    public IAIRequestBuilder<T> Model(string modelId)
    {
        _state.Options.ModelId = modelId;
        return this;
    }

    public IAIRequestBuilder<T> With(string key, object? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        _state.Variables[key] = value;
        return this;
    }

    public IAIRequestBuilder<T> Chat(string conversationId)
    {
        ArgumentException.ThrowIfNullOrEmpty(conversationId);
        _state.ConversationId = conversationId;
        return this;
    }

    public async Task<AIResponse<T>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        AIPreparedRequest prepared = await AIRequestExecutor.PrepareAsync(_state, cancellationToken).ConfigureAwait(false);

        ChatResponse<T> response = await _state.Context.ChatClient
            .GetResponseAsync<T>(prepared.Messages, prepared.Options, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        bool isValid = response.TryGetResult(out T? value);

        await AIRequestExecutor.PersistAsync(_state, prepared.UserText, response.Text, cancellationToken)
            .ConfigureAwait(false);

        return new AIResponse<T>
        {
            Text = response.Text,
            Usage = response.Usage,
            ModelId = response.ModelId,
            FinishReason = response.FinishReason,
            RawResponse = response,
            Value = value,
            IsValid = isValid,
        };
    }
}
