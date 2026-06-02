using Microsoft.Extensions.AI;

namespace Niobium.AI.OpenAI
{
    internal class OpenAIImageClientFactory(OpenAIClientFactory clientFactory) : IImageClientFactory
    {
        public IImageClient CreateClient(string model)
            => model.StartsWith("gpt-image-", StringComparison.OrdinalIgnoreCase)
                ? this.CreateAdaptor(model)
                : throw new NotSupportedException($"The specified image model is not supported: {model}");

        private OpenAIImageClientAdaptor CreateAdaptor(string model)
        {
            IImageGenerator openAIImageClient = clientFactory.GetOrCreateClient()
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
