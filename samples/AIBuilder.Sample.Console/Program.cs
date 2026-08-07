using AIBuilder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

string apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
    ?? throw new InvalidOperationException("Set the OPENAI_API_KEY environment variable.");

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Logging.SetMinimumLevel(LogLevel.Warning);

IAIBuilder ai = builder.Services.AddAIBuilder();

ai.UseOpenAI(apiKey, "gpt-4o-mini")
    .UseLogging()
    .UseRetry()
    .UseRateLimit(o => o.PermitLimit = 60)
    .UseMemoryCache()
    .UseTelemetry()
    .UseTools()
    .UseCost(o =>
    {
        o.AddModel("gpt-4o-mini", inputPerMillionTokens: 0.15m, outputPerMillionTokens: 0.60m);
        o.OnCostComputed = report =>
            Console.WriteLine($"[cost] {report.ModelId}: {report.TotalCost:C4} ({report.InputTokens}+{report.OutputTokens} tokens)");
    });

ai.AddTool((string city) => $"The weather in {city} is sunny, 24°C.", "get_weather", "Gets the current weather for a city.");

ai.AddPromptTemplates(t => t
        .Add("summarize", "Summarize the following text in one sentence:\n\n{{text}}", "You are a concise assistant."))
    .UseInMemoryConversations();

using IHost host = builder.Build();
var client = host.Services.GetRequiredService<IAIClient>();

Console.WriteLine("=== Simple prompt ===");
AIResponse simple = await client.Prompt("Give me a one-sentence fun fact about .NET.").Temperature(0.2f).ExecuteAsync();
Console.WriteLine(simple.Text);

Console.WriteLine("\n=== Prompt template ===");
AIResponse summary = await client.Template("summarize")
    .With("text", "AIBuilder is a composable middleware pipeline for AI applications in .NET.")
    .ExecuteAsync();
Console.WriteLine(summary.Text);

Console.WriteLine("\n=== Tool calling ===");
AIResponse weather = await client.Prompt("What's the weather in Colombo?").WithTools().ExecuteAsync();
Console.WriteLine(weather.Text);

Console.WriteLine("\n=== Conversation memory ===");
await client.Prompt("My name is Ada.").Chat("demo").ExecuteAsync();
AIResponse recall = await client.Prompt("What is my name?").Chat("demo").ExecuteAsync();
Console.WriteLine(recall.Text);

Console.WriteLine("\n=== Streaming ===");
await foreach (AIResponseUpdate update in client.Prompt("Count from 1 to 5.").StreamAsync())
{
    Console.Write(update.Text);
}

Console.WriteLine();
