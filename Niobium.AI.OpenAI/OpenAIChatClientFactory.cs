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
            => this.standardOpenAIClient ??= new OpenAIClient(
                new ApiKeyCredential(Environment.GetEnvironmentVariable("LLM_OPENAI_KEY")!),
                new OpenAIClientOptions
                {
                    Endpoint = new Uri(Environment.GetEnvironmentVariable("LLM_OPENAI_ENDPOINT")!),
                    NetworkTimeout = TimeSpan.FromMinutes(15)
                })
            .GetResponsesClient(model)
            .AsIChatClient();

        private IChatClient CreateQwenChatClient(string model)
            => this.lowCostOpenAIClient ??= new OpenAIClient(
                    new ApiKeyCredential(Environment.GetEnvironmentVariable("LLM_QWEN_KEY")!),
                    new OpenAIClientOptions
                    {
                        Endpoint = new Uri(Environment.GetEnvironmentVariable("LLM_QWEN_ENDPOINT")!),
                        NetworkTimeout = TimeSpan.FromMinutes(15)
                    })
                .GetChatClient(model)
                .AsIChatClient();
    }
}
