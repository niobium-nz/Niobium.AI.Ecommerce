namespace Niobium.AI.OpenAI
{
    internal class OpenAIVideoClientFactory(SoraVideoClient soraVideoClient) : IVideoClientFactory
    {
        public IVideoClient CreateClient(string model, string? provider)
        {
            provider ??= String.Empty;

            if (model.StartsWith("sora", StringComparison.OrdinalIgnoreCase))
            {
                soraVideoClient.Initialize(provider);
                return soraVideoClient;
            }

            throw new NotSupportedException($"The specified LLM model is not supported: {model}");
        }
    }
}
