using System.ClientModel;
using OpenAI;

namespace Niobium.AI.OpenAI
{
    internal class OpenAIClientFactory
    {
        private OpenAIClient? client;

        public OpenAIClient GetOrCreateClient()
        {
            string endpoint = Environment.GetEnvironmentVariable("LLM_OPENAI_ENDPOINT")
                ?? throw new Exception("`LLM_OPENAI_ENDPOINT` must be configured.");
            string key = Environment.GetEnvironmentVariable("LLM_OPENAI_KEY")
                ?? throw new Exception("`LLM_OPENAI_KEY` must be configured.");
            return this.client ??= new OpenAIClient(
                new ApiKeyCredential(key),
                new OpenAIClientOptions
                {
                    Endpoint = new Uri(endpoint),
                    NetworkTimeout = TimeSpan.FromMinutes(10),
                });
        }
    }
}
