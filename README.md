# AIBuilder

The composable **middleware pipeline for AI applications in .NET** — built on top of
[`Microsoft.Extensions.AI`](https://learn.microsoft.com/dotnet/ai/).

Think *Serilog / Polly, but for AI*: your application depends on a stable, provider-agnostic
pipeline instead of any single vendor SDK.

```csharp
builder.Services
    .AddAIBuilder()
    .UseOpenAI(apiKey, "gpt-4o-mini")
    .UseLogging()
    .UseRetry();
```

```csharp
var response = await ai
    .Prompt("Summarize this document")
    .SystemPrompt("You are a concise assistant.")
    .Temperature(0.2f)
    .ExecuteAsync();

Console.WriteLine(response.Text);
```

Structured output:

```csharp
var person = await ai
    .Prompt("Extract the person from: John Doe, 34, London")
    .As<Person>()
    .ExecuteAsync();
```

## Packages

| Package | Purpose |
| --- | --- |
| `AIBuilder.Abstractions` | Core interfaces (`IAIClient`, `IAIRequestBuilder`, `AIResponse`, stores) |
| `AIBuilder.Core` | Fluent client + retry, cost, and rate-limit middleware |
| `AIBuilder.DependencyInjection` | `AddAIBuilder()` + middleware registration |
| `AIBuilder.OpenAI` | OpenAI provider (`UseOpenAI`) |
| `AIBuilder.AzureOpenAI` | Azure OpenAI provider (`UseAzureOpenAI`) |
| `AIBuilder.Ollama` | Ollama / local LLM provider (`UseOllama`) |
| `AIBuilder.Cache` | Response caching (`UseMemoryCache`, `UseDistributedCache`) |
| `AIBuilder.Telemetry` | OpenTelemetry middleware (`UseTelemetry`) |
| `AIBuilder.Tools` | Tool/function calling (`AddTool`, `AddTools<T>`, `UseTools`) |
| `AIBuilder.Prompts` | Reusable prompt templates (`AddPromptTemplates`) |
| `AIBuilder.Memory` | Conversation memory (`UseInMemoryConversations`) |
| `AIBuilder.RAG` | Retrieval-augmented generation (chunking, vector store, retriever) |

## Feature examples

Full pipeline:

```csharp
IAIBuilder ai = builder.Services.AddAIBuilder();

ai.UseOpenAI(apiKey, "gpt-4o-mini")
    .UseLogging()
    .UseRetry()
    .UseRateLimit(o => o.PermitLimit = 60)
    .UseMemoryCache()
    .UseTelemetry()
    .UseTools()
    .UseCost(o => o.AddModel("gpt-4o-mini", 0.15m, 0.60m));

ai.AddTool((string city) => $"Sunny in {city}", "get_weather", "Gets the weather");
ai.AddPromptTemplates(t => t.Add("summarize", "Summarize:\n{{text}}"))
  .UseInMemoryConversations();
```

Tools, templates, memory, and structured output:

```csharp
await ai.Prompt("What's the weather in Colombo?").WithTools().ExecuteAsync();
await ai.Template("summarize").With("text", document).ExecuteAsync();
await ai.Prompt("Remember my name is Ada.").Chat("user-42").ExecuteAsync();
var person = await ai.Prompt(text).As<Person>().ExecuteAsync();
```

Retrieval-augmented generation:

```csharp
services.AddRag();
await pipeline.IngestAsync(DocumentLoader.FromText("doc1", text));
var hits = await retriever.RetrieveAsync("question", topK: 5);
var context = RagContextBuilder.BuildContext(hits);
```

> Providers: OpenAI, Azure OpenAI, Ollama. Middleware: logging, retry, caching, cost,
> rate limiting, telemetry, tools. Plus prompt templates, conversation memory, and RAG.

## Building & testing

```powershell
dotnet build AIBuilder.slnx -c Release          # net8.0 + net9.0 + net10.0, warnings-as-errors
dotnet test  AIBuilder.slnx -c Release          # unit tests (all TFMs) + integration (skipped without a key)
dotnet pack  AIBuilder.slnx -c Release -o artifacts
```

Integration tests run against a live provider and are skipped unless `OPENAI_API_KEY` is set.
CI (build + multi-TFM test + pack) is defined in `.github/workflows/ci.yml`.

Before publishing to NuGet, set your repository URL:

```powershell
dotnet pack -c Release /p:RepositoryUrl=https://github.com/<owner>/<repo>
```

## Status & known limitations

- **Retry** applies to non-streaming responses only; streaming is passed through.
- **Rate limiting** is per-process (in-memory), not distributed.
- **Conversation memory** and the **RAG vector store** are in-memory; external backends
  (Redis, SQL, Cosmos DB, pgvector) are not yet included.
- **Structured output** and **tool calling** use runtime reflection and are **not** NativeAOT/trim-safe.
  The rest of the pipeline is reflection-free.


