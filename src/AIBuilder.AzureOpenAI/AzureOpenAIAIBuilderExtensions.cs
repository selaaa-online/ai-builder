using System.ClientModel;
using AIBuilder;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Azure OpenAI provider registration extensions for <see cref="IAIBuilder"/>.
/// </summary>
public static class AzureOpenAIAIBuilderExtensions
{
    /// <summary>
    /// Registers Azure OpenAI as the AI provider using an API key.
    /// </summary>
    /// <param name="builder">The AIBuilder instance.</param>
    /// <param name="endpoint">The Azure OpenAI resource endpoint.</param>
    /// <param name="apiKey">The Azure OpenAI API key.</param>
    /// <param name="deploymentName">The deployment (model) name.</param>
    public static IAIBuilder UseAzureOpenAI(
        this IAIBuilder builder,
        Uri endpoint,
        string apiKey,
        string deploymentName)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(deploymentName);

        var azureClient = new AzureOpenAIClient(endpoint, new ApiKeyCredential(apiKey));
        IChatClient chatClient = azureClient.GetChatClient(deploymentName).AsIChatClient();

        return builder.UseChatClient(chatClient);
    }

    /// <summary>
    /// Registers Azure OpenAI using a pre-configured <see cref="AzureOpenAIClient"/>.
    /// </summary>
    public static IAIBuilder UseAzureOpenAI(
        this IAIBuilder builder,
        AzureOpenAIClient client,
        string deploymentName)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(client);
        ArgumentException.ThrowIfNullOrWhiteSpace(deploymentName);

        IChatClient chatClient = client.GetChatClient(deploymentName).AsIChatClient();
        return builder.UseChatClient(chatClient);
    }
}
