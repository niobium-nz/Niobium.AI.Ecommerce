namespace Niobium.AI.OpenAI
{
    internal class OpenAIVideoClientFactory(SoraVideoClient soraVideoClient) : IVideoClientFactory
    {
        public IVideoClient CreateClient(string model)
            => model.StartsWith("sora", StringComparison.OrdinalIgnoreCase) ? (IVideoClient)soraVideoClient
            : throw new NotSupportedException($"The specified LLM model is not supported: {model}");
    }
}
