using LLama;
using LLama.Abstractions;
using LLama.Common;
using Microsoft.Extensions.AI;

namespace Inference.LlamaServer;

public static class LlamaService
{
    public static IChatClient CreateChatClient()
    {
        var parameters = new ModelParams(Path.Combine(AppContext.BaseDirectory, "Llama-3.2-3B-Instruct-Q4_0.gguf"))
        {
            ContextSize = 32768
        };
        var model = LLamaWeights.LoadFromFile(parameters);
        return new StatelessExecutor(model, parameters) { ApplyTemplate = true }.AsChatClient();
    }
}
