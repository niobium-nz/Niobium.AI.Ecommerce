using System.ClientModel;
using Microsoft.Extensions.Options;
using OpenAI;

namespace Niobium.AI.OpenAI
{
    internal class OpenAIClientFactory(IOptions<OpenAIClientOptions> options)
    {
        private OpenAIClient? client;

        public OpenAIClient GetOrCreateClient() 
            => this.client ??= new OpenAIClient(
                new ApiKeyCredential(options.Value.LLMKey),
                new global::OpenAI.OpenAIClientOptions
                {
                    Endpoint = new Uri(options.Value.LLMEndpoint),
                    NetworkTimeout = TimeSpan.FromMinutes(15),
                });
    }
}
