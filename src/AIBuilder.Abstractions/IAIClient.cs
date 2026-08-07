namespace AIBuilder;

/// <summary>
/// Entry point for issuing AI requests through the AIBuilder pipeline.
/// </summary>
public interface IAIClient
{
    /// <summary>Begins building a request with the specified user prompt.</summary>
    /// <param name="prompt">The user prompt text.</param>
    /// <returns>A fluent request builder.</returns>
    IAIRequestBuilder Prompt(string prompt);

    /// <summary>Begins building a request from a named prompt template.</summary>
    /// <param name="templateName">The name of a registered template.</param>
    /// <returns>A fluent request builder. Supply variables with <c>With(...)</c>.</returns>
    IAIRequestBuilder Template(string templateName);
}
