using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Niobium.AI.OpenAI
{
    internal class OpenAIChatClientFactory(OpenAIClientFactory clientFactory, IOptions<OpenAIClientOptions> retryOptions, ILogger<OpenAIChatClientFactory> logger) : IChatClientFactory
    {
        public IChatClient CreateChatClient(string model)
        {
            if (!model.StartsWith("gpt-", StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException($"The specified LLM model is not supported: {model}");
            }

            IChatClient chatClient = clientFactory.GetOrCreateClient()
                .GetResponsesClient()
                .AsIChatClient(model)
                .AsBuilder()
                .UseOpenTelemetry(
                    sourceName: this.GetType().Assembly.GetName().Name,
                    configure: (cfg) => cfg.EnableSensitiveData = true)
                .Build();

            return retryOptions.Value.RetryKeywords.Count > 0 && retryOptions.Value.MaxRetries > 0
                ? new KeywordRetryChatClient(chatClient, retryOptions.Value, logger)
                : chatClient;
        }
    }
}
