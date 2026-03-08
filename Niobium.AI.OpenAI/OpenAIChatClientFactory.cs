using System.ClientModel;
using Microsoft.Extensions.AI;
using OpenAI;

namespace Niobium.AI.OpenAI
{
    internal class OpenAIChatClientFactory : IChatClientFactory
    {
        private IChatClient? standardOpenAIClient;
        private IChatClient? lowCostOpenAIClient;

        public IChatClient CreateChatClient(string model)
        {
            if (model.StartsWith("gpt-", StringComparison.OrdinalIgnoreCase))
            {
                return CreateOpenAIChatClient(model);
            }
            else if (model.StartsWith("qwen", StringComparison.OrdinalIgnoreCase))
            {
                return CreateQwenChatClient(model);
            }
            else
            {
                throw new NotSupportedException($"The specified LLM model is not supported: {model}");
            }
        }

        private IChatClient CreateOpenAIChatClient(string model)
        {
            var endpoint = Environment.GetEnvironmentVariable("LLM_OPENAI_ENDPOINT")
                ?? throw new Exception("`LLM_OPENAI_ENDPOINT` must be configured.");
            var key = Environment.GetEnvironmentVariable("LLM_OPENAI_KEY")
                ?? throw new Exception("`LLM_OPENAI_KEY` must be configured.");
            return this.standardOpenAIClient ??= new OpenAIClient(
                new ApiKeyCredential(key),
                new OpenAIClientOptions
                {
                    Endpoint = new Uri(endpoint),
                    NetworkTimeout = TimeSpan.FromMinutes(15)
                })
                .GetResponsesClient(model)
                .AsIChatClient();
        }

        private IChatClient CreateQwenChatClient(string model)
        {
            var endpoint = Environment.GetEnvironmentVariable("LLM_QWEN_ENDPOINT")
                ?? throw new Exception("`LLM_QWEN_ENDPOINT` must be configured.");
            var key = Environment.GetEnvironmentVariable("LLM_QWEN_KEY")
                ?? throw new Exception("`LLM_QWEN_KEY` must be configured.");
            return this.lowCostOpenAIClient ??= new OpenAIClient(
                new ApiKeyCredential(key),
                new OpenAIClientOptions
                {
                    Endpoint = new Uri(endpoint),
                    NetworkTimeout = TimeSpan.FromMinutes(15)
                })
                .GetChatClient(model)
                .AsIChatClient();
        }
    }
}
