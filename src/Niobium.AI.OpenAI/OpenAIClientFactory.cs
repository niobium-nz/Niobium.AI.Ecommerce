using System.ClientModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OpenAI;

namespace Niobium.AI.OpenAI
{
    internal class OpenAIClientFactory(IOptions<OpenAIOptions> options, IConfiguration configuration)
    {
        private OpenAIClient? client;

        public OpenAIClient GetOrCreateClient(string provider)
        {
            string? endpoint = options.Value.ResponseEndpoint;
            string? key = options.Value.ResponseEndpointKey;

            if (!String.IsNullOrWhiteSpace(provider))
            {
                IConfigurationSection config = configuration.GetRequiredSection(OpenAIOptions.SectionName);
                endpoint = config.GetValue<string>($"{provider.Trim().ToUpperInvariant()}{nameof(OpenAIOptions.ResponseEndpoint).ToUpperInvariant()}");
                key = config.GetValue<string>($"{provider.Trim().ToUpperInvariant()}{nameof(OpenAIOptions.ResponseEndpointKey).ToUpperInvariant()}");
            }

            if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Response endpoint and key must be configured.");
            }

            return this.client ??= new OpenAIClient(
                new ApiKeyCredential(key),
                new OpenAIClientOptions
                {
                    Endpoint = new Uri(endpoint),
                    NetworkTimeout = TimeSpan.FromMinutes(15),
                });
        }
    }
}
