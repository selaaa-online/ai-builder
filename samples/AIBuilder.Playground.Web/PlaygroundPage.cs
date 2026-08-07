namespace AIBuilder;

internal static class PlaygroundPage
{
    public const string Html = """
        <!DOCTYPE html>
        <html lang="en">
        <head>
          <meta charset="utf-8" />
          <meta name="viewport" content="width=device-width, initial-scale=1" />
          <title>AIBuilder Playground</title>
          <style>
            body { font-family: system-ui, sans-serif; max-width: 780px; margin: 2rem auto; padding: 0 1rem; }
            h1 { font-size: 1.4rem; }
            textarea { width: 100%; min-height: 90px; padding: .6rem; font: inherit; }
            button { padding: .5rem 1rem; font: inherit; cursor: pointer; }
            .pill { display: inline-block; background: #eef; border-radius: 999px; padding: .15rem .6rem; margin: .1rem; font-size: .8rem; }
            .out { white-space: pre-wrap; background: #f6f6f6; border-radius: 8px; padding: 1rem; margin-top: 1rem; }
            .meta { color: #555; font-size: .85rem; margin-top: .5rem; }
          </style>
        </head>
        <body>
          <h1>AIBuilder Playground</h1>
          <div id="pipeline"></div>
          <p><textarea id="prompt" placeholder="Ask something...">Give me a one-sentence fun fact about .NET.</textarea></p>
          <button onclick="run()">Send</button>
          <div id="output" class="out" hidden></div>
          <div id="meta" class="meta"></div>
          <script>
            async function loadPipeline() {
              const r = await fetch('/api/pipeline');
              const d = await r.json();
              document.getElementById('pipeline').innerHTML =
                d.middleware.map(m => `<span class="pill">${m}</span>`).join('') +
                (d.providerConfigured ? '' : '<p style="color:#a00">No API key configured. Set OPENAI_API_KEY.</p>');
            }
            async function run() {
              const out = document.getElementById('output');
              const meta = document.getElementById('meta');
              out.hidden = false; out.textContent = 'Thinking...'; meta.textContent = '';
              const r = await fetch('/api/chat', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ prompt: document.getElementById('prompt').value })
              });
              if (!r.ok) { out.textContent = 'Error: ' + (await r.text()); return; }
              const d = await r.json();
              out.textContent = d.text;
              meta.textContent = `model: ${d.model ?? '?'} · tokens: ${d.inputTokens ?? '?'}+${d.outputTokens ?? '?'}` +
                (d.totalCost != null ? ` · cost: $${Number(d.totalCost).toFixed(5)}` : '');
            }
            loadPipeline();
          </script>
        </body>
        </html>
        """;
}
