using Microsoft.Extensions.AI;

namespace Niobium.AI.OpenAI
{
    internal class OpenAIChatClientFactory(OpenAIClientFactory clientFactory) : IChatClientFactory
    {
        public IChatClient CreateChatClient(string model)
            => model.StartsWith("gpt-", StringComparison.OrdinalIgnoreCase)
                ? clientFactory.GetOrCreateClient()
                    .GetResponsesClient()
                    .AsIChatClient(model)
                    .AsBuilder()
                    .UseOpenTelemetry(
                        sourceName: this.GetType().Assembly.GetName().Name,
                        configure: (cfg) => cfg.EnableSensitiveData = true)
                    .Build()
                : throw new NotSupportedException($"The specified LLM model is not supported: {model}");
    }
}
