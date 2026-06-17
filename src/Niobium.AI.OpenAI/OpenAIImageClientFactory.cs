using Microsoft.Extensions.AI;

namespace Niobium.AI.OpenAI
{
    internal class OpenAIImageClientFactory(OpenAIClientFactory clientFactory) : IImageClientFactory
    {
        public IImageClient CreateClient(string model, string? provider)
            => model.StartsWith("gpt-image-", StringComparison.OrdinalIgnoreCase)
                ? this.CreateAdaptor(model, provider ?? string.Empty)
                : throw new NotSupportedException($"The specified image model is not supported: {model}");

        private OpenAIImageClientAdaptor CreateAdaptor(string model, string provider)
        {
            IImageGenerator openAIImageClient = clientFactory.GetOrCreateClient(provider)
                .GetImageClient(model)
                .AsIImageGenerator()
                .AsBuilder()
                .UseOpenTelemetry(
                    sourceName: this.GetType().Assembly.GetName().Name,
                    configure: (cfg) => cfg.EnableSensitiveData = true)
                .Build();
            return new OpenAIImageClientAdaptor(openAIImageClient);
        }
    }
}
