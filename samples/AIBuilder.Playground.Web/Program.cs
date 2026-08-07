using AIBuilder;
using AIBuilder.Middleware;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string? apiKey = builder.Configuration["OpenAI:ApiKey"]
    ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");

string[] pipeline = ["Logging", "Retry", "MemoryCache", "Telemetry", "Cost", "Provider(OpenAI)"];

IAIBuilder ai = builder.Services.AddAIBuilder();
if (!string.IsNullOrWhiteSpace(apiKey))
{
    ai.UseOpenAI(apiKey, "gpt-4o-mini")
        .UseLogging()
        .UseRetry()
        .UseMemoryCache()
        .UseTelemetry()
        .UseCost(o => o.AddModel("gpt-4o-mini", inputPerMillionTokens: 0.15m, outputPerMillionTokens: 0.60m));
}

WebApplication app = builder.Build();

app.MapGet("/", () => Results.Content(PlaygroundPage.Html, "text/html"));

app.MapGet("/api/pipeline", () => Results.Json(new
{
    middleware = pipeline,
    providerConfigured = !string.IsNullOrWhiteSpace(apiKey),
}));

app.MapPost("/api/chat", async (ChatRequest request, IServiceProvider services, CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(apiKey))
    {
        return Results.Problem("No OpenAI API key configured. Set the OPENAI_API_KEY environment variable and restart.");
    }

    if (string.IsNullOrWhiteSpace(request.Prompt))
    {
        return Results.BadRequest(new { error = "Prompt is required." });
    }

    var client = services.GetRequiredService<IAIClient>();
    AIResponse response = await client
        .Prompt(request.Prompt)
        .Temperature(request.Temperature ?? 0.2f)
        .ExecuteAsync(cancellationToken);

    CostReport? cost = null;
    if (response.RawResponse?.AdditionalProperties is { } props
        && props.TryGetValue(CostOptions.ResponsePropertyKey, out object? value))
    {
        cost = value as CostReport;
    }

    return Results.Json(new
    {
        text = response.Text,
        model = response.ModelId,
        inputTokens = response.Usage?.InputTokenCount,
        outputTokens = response.Usage?.OutputTokenCount,
        totalCost = cost?.TotalCost,
    });
});

app.Run();

internal sealed record ChatRequest(string Prompt, float? Temperature);
