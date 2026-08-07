using Microsoft.Extensions.AI;

namespace AIBuilder;

/// <summary>
/// Fluent builder for configuring and executing an AI request.
/// </summary>
public interface IAIRequestBuilder
{
    /// <summary>Sets the system prompt for the request.</summary>
    IAIRequestBuilder SystemPrompt(string systemPrompt);

    /// <summary>Adds additional context, supplied to the model as a user message before the prompt.</summary>
    IAIRequestBuilder WithContext(string context);

    /// <summary>Sets the sampling temperature.</summary>
    IAIRequestBuilder Temperature(float temperature);

    /// <summary>Sets the maximum number of output tokens.</summary>
    IAIRequestBuilder MaxTokens(int maxTokens);

    /// <summary>Overrides the model identifier for this request.</summary>
    IAIRequestBuilder Model(string modelId);

    /// <summary>Sets a template variable value. Only used when the request originates from a template.</summary>
    IAIRequestBuilder With(string key, object? value);

    /// <summary>Associates the request with a conversation, loading and persisting history.</summary>
    IAIRequestBuilder Chat(string conversationId);

    /// <summary>
    /// Attaches tools to the request. When called with no arguments, all tools registered
    /// in dependency injection are used.
    /// </summary>
    IAIRequestBuilder WithTools(params AITool[] tools);

    /// <summary>Requests a strongly-typed, structured response of type <typeparamref name="T"/>.</summary>
    IAIRequestBuilder<T> As<T>();

    /// <summary>Executes the request and returns the response.</summary>
    Task<AIResponse> ExecuteAsync(CancellationToken cancellationToken = default);

    /// <summary>Executes the request and streams incremental response updates.</summary>
    IAsyncEnumerable<AIResponseUpdate> StreamAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Fluent builder for a request that returns a strongly-typed, structured response.
/// </summary>
/// <typeparam name="T">The type to deserialize the model response into.</typeparam>
public interface IAIRequestBuilder<T>
{
    /// <summary>Sets the system prompt for the request.</summary>
    IAIRequestBuilder<T> SystemPrompt(string systemPrompt);

    /// <summary>Adds additional context, supplied to the model as a user message before the prompt.</summary>
    IAIRequestBuilder<T> WithContext(string context);

    /// <summary>Sets the sampling temperature.</summary>
    IAIRequestBuilder<T> Temperature(float temperature);

    /// <summary>Sets the maximum number of output tokens.</summary>
    IAIRequestBuilder<T> MaxTokens(int maxTokens);

    /// <summary>Overrides the model identifier for this request.</summary>
    IAIRequestBuilder<T> Model(string modelId);

    /// <summary>Sets a template variable value. Only used when the request originates from a template.</summary>
    IAIRequestBuilder<T> With(string key, object? value);

    /// <summary>Associates the request with a conversation, loading and persisting history.</summary>
    IAIRequestBuilder<T> Chat(string conversationId);

    /// <summary>Executes the request and returns the strongly-typed response.</summary>
    Task<AIResponse<T>> ExecuteAsync(CancellationToken cancellationToken = default);
}
