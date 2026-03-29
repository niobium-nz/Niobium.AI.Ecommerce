using System.ClientModel;
using Microsoft.Extensions.AI;
using OpenAI;

namespace Niobium.AI.OpenAI
{
    internal class OpenAIChatClientFactory : IChatClientFactory
    {
        private IChatClient? standardOpenAIClient;
        private IChatClient? lowCostOpenAIClient;

        public IChatClient CreateChatClient(string model) => model.StartsWith("gpt-", StringComparison.OrdinalIgnoreCase)
                ? this.CreateOpenAIChatClient(model)
                : model.StartsWith("qwen", StringComparison.OrdinalIgnoreCase)
                    ? this.CreateQwenChatClient(model)
                    : throw new NotSupportedException($"The specified LLM model is not supported: {model}");

        private IChatClient CreateOpenAIChatClient(string model)
        {
            string endpoint = Environment.GetEnvironmentVariable("LLM_OPENAI_ENDPOINT")
                ?? throw new Exception("`LLM_OPENAI_ENDPOINT` must be configured.");
            string key = Environment.GetEnvironmentVariable("LLM_OPENAI_KEY")
                ?? throw new Exception("`LLM_OPENAI_KEY` must be configured.");
            return this.standardOpenAIClient ??= new OpenAIClient(
                new ApiKeyCredential(key),
                new OpenAIClientOptions
                {
                    Endpoint = new Uri(endpoint),
                })
                .GetResponsesClient(model)
                .AsIChatClient();
        }

        private IChatClient CreateQwenChatClient(string model)
        {
            string endpoint = Environment.GetEnvironmentVariable("LLM_QWEN_ENDPOINT")
                ?? throw new Exception("`LLM_QWEN_ENDPOINT` must be configured.");
            string key = Environment.GetEnvironmentVariable("LLM_QWEN_KEY")
                ?? throw new Exception("`LLM_QWEN_KEY` must be configured.");
            return this.lowCostOpenAIClient ??= new OpenAIClient(
                new ApiKeyCredential(key),
                new OpenAIClientOptions
                {
                    Endpoint = new Uri(endpoint),
                })
                .GetChatClient(model)
                .AsIChatClient();
        }
    }
}
