using Inference.LlamaServer;
using Microsoft.Extensions.AI;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddSingleton(LlamaService.CreateChatClient());
builder.Services.AddSingleton(new SemaphoreSlim(1, 1));

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.MapPost("/chat", (ChatRequest request, IChatClient chat, SemaphoreSlim gate, CancellationToken ct) =>
{
    async IAsyncEnumerable<string> Stream()
    {
        await gate.WaitAsync(ct);
        try
        {
            await foreach (var update in chat.GetStreamingResponseAsync(request.Prompt, cancellationToken: ct))
            {
                if (update.Text is { Length: > 0 } text)
                    yield return text;
            }
        }
        finally
        {
            gate.Release();
        }
    }

    return TypedResults.ServerSentEvents(Stream());
});

app.Run();

record ChatRequest(string Prompt);
