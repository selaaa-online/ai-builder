# AIBuilder

[![CI](https://github.com/selaaa-online/ai-builder/actions/workflows/ci.yml/badge.svg)](https://github.com/selaaa-online/ai-builder/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0%20%7C%2010.0-512BD4)](https://dotnet.microsoft.com/)

**AIBuilder is a composable middleware pipeline for AI applications in .NET**, built on top of
[`Microsoft.Extensions.AI`](https://learn.microsoft.com/dotnet/ai/).

Think **Serilog or Polly, but for AI.** Instead of scattering retries, caching, logging,
token counting, and vendor SDK calls throughout your app, you compose them into a single
pipeline — and your code depends on that **stable, provider-agnostic pipeline** rather than
on any one AI vendor's SDK. Switching from OpenAI to Azure OpenAI or a local model becomes a
one-line change.

```csharp
// 1. Configure the pipeline once (Program.cs)
builder.Services
    .AddAIBuilder()
    .UseOpenAI(apiKey, "gpt-4o-mini")
    .UseLogging()
    .UseRetry()
    .UseMemoryCache()
    .UseCost(o => o.AddModel("gpt-4o-mini", 0.15m, 0.60m));

// 2. Use it anywhere via IAIClient
var response = await ai
    .Prompt("Summarize this document")
    .SystemPrompt("You are a concise assistant.")
    .Temperature(0.2f)
    .ExecuteAsync();

Console.WriteLine(response.Text);
```

---

## Table of contents

- [Why AIBuilder](#why-aibuilder)
- [How it works](#how-it-works)
- [Installation](#installation)
- [Quick start](#quick-start)
- [Providers](#providers)
- [The fluent request API](#the-fluent-request-api)
- [Middleware](#middleware)
- [Features](#features)
  - [Structured output](#structured-output)
  - [Streaming](#streaming)
  - [Tool / function calling](#tool--function-calling)
  - [Prompt templates](#prompt-templates)
  - [Conversation memory](#conversation-memory)
  - [Retrieval-augmented generation (RAG)](#retrieval-augmented-generation-rag)
- [The response object](#the-response-object)
- [Configuration reference](#configuration-reference)
- [Building, testing & publishing](#building-testing--publishing)
- [Status & known limitations](#status--known-limitations)
- [License](#license)

---

## Why AIBuilder

Modern AI apps repeat the same plumbing over and over. AIBuilder gives you that plumbing as
reusable, ordered middleware so you can stop writing it by hand:

- **Retries** with exponential backoff for rate limits and transient failures
- **Response caching** (in-memory or distributed / Redis)
- **Cost tracking** — input/output token cost per request
- **Rate limiting** to stay under provider quotas
- **Structured logging** and **OpenTelemetry** tracing/metrics
- **Tool / function calling** with automatic invocation
- **Structured output** — get a typed object back instead of raw JSON
- **Prompt templates**, **conversation memory**, and **RAG** building blocks

Because everything sits behind a provider-agnostic pipeline, your application code never
references a vendor SDK directly.

## How it works

AIBuilder mirrors the ASP.NET Core request pipeline. A request flows through the middleware
you register, reaches the provider, and the response flows back out:

```
        Your app  ──►  IAIClient (fluent request)
                              │
   ┌──────────────────────────────────────────────┐
   │  Logging → Retry → RateLimit → Cache → Cost   │   ← middleware you compose
   │           → Telemetry → Tools                 │
   └──────────────────────────────────────────────┘
                              │
                     Provider adapter
        (OpenAI · Azure OpenAI · Ollama · …)
                              │
                         AI response
```

Under the hood each middleware is a `Microsoft.Extensions.AI` `DelegatingChatClient`, and the
providers are standard `IChatClient` implementations — so AIBuilder composes with the wider
.NET AI ecosystem rather than replacing it.

## Installation

AIBuilder is split into small packages so you only pull in what you use. Installing a provider
brings in the core pipeline transitively.

```powershell
# Minimal: OpenAI provider + core pipeline
dotnet add package AIBuilder.OpenAI

# Add the middleware / features you need
dotnet add package AIBuilder.Cache
dotnet add package AIBuilder.Telemetry
dotnet add package AIBuilder.Tools
dotnet add package AIBuilder.Prompts
dotnet add package AIBuilder.Memory
dotnet add package AIBuilder.RAG
```

| Package | Purpose | Key API |
| --- | --- | --- |
| `AIBuilder.Abstractions` | Interfaces and models | `IAIClient`, `IAIRequestBuilder`, `AIResponse` |
| `AIBuilder.Core` | Fluent client + retry, cost, rate-limit middleware | `AIClient`, `RetryChatClient` |
| `AIBuilder.DependencyInjection` | DI + middleware registration | `AddAIBuilder()`, `UseRetry()`, `UseCost()` |
| `AIBuilder.OpenAI` | OpenAI provider | `UseOpenAI()` |
| `AIBuilder.AzureOpenAI` | Azure OpenAI provider | `UseAzureOpenAI()` |
| `AIBuilder.Ollama` | Ollama / local LLM provider | `UseOllama()` |
| `AIBuilder.Cache` | Response caching | `UseMemoryCache()`, `UseDistributedCache()` |
| `AIBuilder.Telemetry` | OpenTelemetry | `UseTelemetry()` |
| `AIBuilder.Tools` | Tool / function calling | `AddTool()`, `AddTools<T>()`, `UseTools()` |
| `AIBuilder.Prompts` | Reusable prompt templates | `AddPromptTemplates()` |
| `AIBuilder.Memory` | Conversation memory | `UseInMemoryConversations()` |
| `AIBuilder.RAG` | Retrieval-augmented generation | `AddRag()`, `RagPipeline`, `IRetriever` |

**Target frameworks:** `net8.0`, `net9.0`, `net10.0`.

## Quick start

### Console app

```csharp
using AIBuilder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

string apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")!;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddAIBuilder()
    .UseOpenAI(apiKey, "gpt-4o-mini")
    .UseLogging()
    .UseRetry();

using IHost host = builder.Build();
var ai = host.Services.GetRequiredService<IAIClient>();

AIResponse response = await ai.Prompt("Give me a fun fact about .NET.").ExecuteAsync();
Console.WriteLine(response.Text);
```

### ASP.NET Core (Minimal API)

```csharp
using AIBuilder;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAIBuilder()
    .UseOpenAI(builder.Configuration["OpenAI:ApiKey"]!, "gpt-4o-mini")
    .UseLogging()
    .UseRetry()
    .UseMemoryCache();

var app = builder.Build();

// IAIClient is injected from DI
app.MapPost("/chat", async (ChatRequest req, IAIClient ai) =>
{
    var response = await ai.Prompt(req.Message).ExecuteAsync();
    return Results.Ok(new { answer = response.Text });
});

app.Run();

record ChatRequest(string Message);
```

## Providers

Switching providers only changes the `Use…` call — your request code stays identical.

```csharp
// OpenAI
ai.UseOpenAI(apiKey, "gpt-4o-mini");

// Azure OpenAI
ai.UseAzureOpenAI(new Uri("https://my-resource.openai.azure.com"), apiKey, "my-deployment");

// Ollama (local models)
ai.UseOllama("llama3.2", "http://localhost:11434");
```

Each provider also accepts a pre-configured client for advanced scenarios, e.g.
`UseOpenAI(OpenAIClient client, string modelId)`.

## The fluent request API

Resolve `IAIClient` from DI, then build a request fluently. Every method returns the builder
so calls chain, and nothing runs until you call `ExecuteAsync` / `StreamAsync`.

```csharp
var response = await ai
    .Prompt("Translate to French: Good morning")
    .SystemPrompt("You are a professional translator.")
    .Temperature(0.2f)
    .MaxTokens(200)
    .ExecuteAsync(cancellationToken);
```

| Method | Description |
| --- | --- |
| `Prompt(text)` | Start a request with a user prompt |
| `Template(name)` | Start a request from a registered prompt template |
| `SystemPrompt(text)` | Set the system prompt |
| `WithContext(text)` | Add extra context before the prompt |
| `Temperature(value)` | Sampling temperature |
| `MaxTokens(n)` | Maximum output tokens |
| `Model(id)` | Override the model for this request |
| `With(key, value)` | Supply a template variable |
| `Chat(conversationId)` | Load & persist conversation history |
| `WithTools(...)` | Attach tools (no args = all registered tools) |
| `As<T>()` | Request typed, structured output |
| `ExecuteAsync(ct)` | Execute and return an `AIResponse` |
| `StreamAsync(ct)` | Stream incremental `AIResponseUpdate`s |

## Middleware

Register middleware on the `IAIBuilder` returned by `AddAIBuilder()`. **Order matters** — the
first registered runs outermost. A provider (`UseOpenAI`, etc.) must be registered before any
middleware.

```csharp
builder.Services
    .AddAIBuilder()
    .UseOpenAI(apiKey, "gpt-4o-mini")
    .UseLogging()                                   // structured logs
    .UseRetry(o => o.MaxRetries = 3)                // exponential backoff
    .UseRateLimit(o => o.PermitLimit = 60)          // 60 requests / minute
    .UseMemoryCache()                               // cache identical requests
    .UseTelemetry()                                 // OpenTelemetry traces + metrics
    .UseTools()                                      // enable automatic tool invocation
    .UseCost(o => o.AddModel("gpt-4o-mini", 0.15m, 0.60m));  // per-request cost
```

| Middleware | What it does |
| --- | --- |
| `UseLogging()` | Logs model, provider, duration, token usage, and status (needs an `ILoggerFactory`) |
| `UseRetry(...)` | Retries transient failures (HTTP 408/429/5xx) with exponential backoff + jitter |
| `UseRateLimit(...)` | Throttles requests using a sliding window |
| `UseMemoryCache()` | Caches responses in an in-process store; `UseDistributedCache()` for Redis/SQL |
| `UseCost(...)` | Computes input/output token cost and attaches a `CostReport` to each response |
| `UseTelemetry(...)` | Emits OpenTelemetry spans and metrics under the `AIBuilder` source |
| `UseTools()` | Enables automatic execution of registered tools |

## Features

### Structured output

Ask for a typed result and AIBuilder generates the schema, validates, and deserializes:

```csharp
public record Person(string Name, int Age, string City);

AIResponse<Person> result = await ai
    .Prompt("Extract the person from: John Doe, 34, London")
    .As<Person>()
    .ExecuteAsync();

if (result.IsValid)
    Console.WriteLine($"{result.Value!.Name} lives in {result.Value.City}");
```

### Streaming

```csharp
await foreach (AIResponseUpdate update in ai.Prompt("Write a haiku about .NET").StreamAsync())
{
    Console.Write(update.Text);
}
```

### Tool / function calling

Register tools, enable invocation with `UseTools()`, then attach them per request with
`WithTools()`. AIBuilder runs the requested tools and feeds results back to the model until it
produces a final answer.

```csharp
builder.Services
    .AddAIBuilder()
    .UseOpenAI(apiKey, "gpt-4o-mini")
    .UseTools()
    .AddTool((string city) => $"The weather in {city} is sunny, 24°C.",
             "get_weather", "Gets the current weather for a city.");

var response = await ai.Prompt("What's the weather in Colombo?").WithTools().ExecuteAsync();
```

You can also register every public method of a class as tools with `AddTools<MyToolClass>()`.

### Prompt templates

```csharp
builder.Services
    .AddAIBuilder()
    .UseOpenAI(apiKey, "gpt-4o-mini")
    .AddPromptTemplates(t => t
        .Add("summarize", "Summarize the text:\n\n{{text}}", systemPrompt: "Be concise."));

var summary = await ai.Template("summarize").With("text", document).ExecuteAsync();
```

### Conversation memory

Give a request a conversation id and AIBuilder loads prior turns before the call and saves the
new turn afterwards:

```csharp
builder.Services.AddAIBuilder().UseOpenAI(apiKey, "gpt-4o-mini").UseInMemoryConversations();

await ai.Prompt("My name is Ada.").Chat("user-42").ExecuteAsync();
var reply = await ai.Prompt("What's my name?").Chat("user-42").ExecuteAsync(); // → "Ada"
```

### Retrieval-augmented generation (RAG)

Provider-agnostic building blocks: chunking, embeddings, an in-memory vector store, and a
retriever. Register an `IEmbeddingGenerator` (any `Microsoft.Extensions.AI` embedding client)
plus `AddRag()`, ingest documents, then retrieve relevant context for a prompt.

```csharp
using Microsoft.Extensions.AI;
using OpenAI;

builder.Services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(
    new OpenAIClient(apiKey).GetEmbeddingClient("text-embedding-3-small").AsIEmbeddingGenerator());

builder.Services.AddRag(o => { o.ChunkSize = 1000; o.ChunkOverlap = 150; });

// ingest
var pipeline = sp.GetRequiredService<RagPipeline>();
await pipeline.IngestAsync(DocumentLoader.FromText("doc1", documentText));

// retrieve + ground a prompt
var retriever = sp.GetRequiredService<IRetriever>();
var hits = await retriever.RetrieveAsync("What is AIBuilder?", topK: 5);
string context = RagContextBuilder.BuildContext(hits);

var answer = await ai.Prompt("What is AIBuilder?").WithContext(context).ExecuteAsync();
```

## The response object

```csharp
AIResponse response = await ai.Prompt("…").ExecuteAsync();
```

| Member | Type | Description |
| --- | --- | --- |
| `Text` | `string` | The generated text |
| `Usage` | `UsageDetails?` | Token usage, when reported by the provider |
| `ModelId` | `string?` | The model that produced the response |
| `FinishReason` | `ChatFinishReason?` | Why generation stopped |
| `RawResponse` | `ChatResponse?` | The underlying `Microsoft.Extensions.AI` response |
| `Value` *(on `AIResponse<T>`)* | `T?` | The deserialized structured result |
| `IsValid` *(on `AIResponse<T>`)* | `bool` | Whether deserialization succeeded |

When `UseCost` is enabled, a `CostReport` (model, token counts, input/output/total cost) is
attached to `RawResponse.AdditionalProperties[CostOptions.ResponsePropertyKey]`.

## Configuration reference

**RetryOptions** — `MaxRetries` (3), `BaseDelay` (500 ms), `MaxDelay` (30 s), `ShouldRetry`
(custom predicate).

**RateLimitOptions** — `PermitLimit` (60), `Window` (1 min), `MaxWait` (1 min).

**CostOptions** — `AddModel(id, inputPerMillion, outputPerMillion)`, `Currency` ("USD"),
`OnCostComputed` (callback). Dated model ids (e.g. `gpt-4o-mini-2024-07-18`) match the closest
configured prefix.

**RagOptions** — `ChunkSize` (1000), `ChunkOverlap` (100).

## Building, testing & publishing

```powershell
dotnet build AIBuilder.slnx -c Release      # net8.0 + net9.0 + net10.0, warnings-as-errors
dotnet test  AIBuilder.slnx -c Release      # unit tests (all TFMs); integration skipped without a key
dotnet pack  AIBuilder.slnx -c Release -o artifacts
```

- **Unit tests** run on every target framework and use an in-memory `IChatClient`.
- **Integration tests** hit a live provider and are skipped unless `OPENAI_API_KEY` is set.
- **CI** (`.github/workflows/ci.yml`) restores, builds, tests all TFMs, and packs on every push/PR.
- **Publishing** — pushing a `v*.*.*` tag runs `.github/workflows/release.yml`, which packs and
  runs `dotnet nuget push` using a `NUGET_API_KEY` repository secret:

  ```powershell
  git tag v1.0.0
  git push origin v1.0.0
  ```

## Status & known limitations

- **Retry** applies to non-streaming responses only; streaming is passed through.
- **Rate limiting** is per-process (in-memory), not distributed across instances.
- **Conversation memory** and the **RAG vector store** are in-memory; external backends
  (Redis, SQL, Cosmos DB, pgvector) are not yet included.
- **Structured output** and **tool calling** use runtime reflection and are **not**
  NativeAOT/trim-safe. The rest of the pipeline is reflection-free.

## License

[MIT](LICENSE) © AIBuilder contributors.


