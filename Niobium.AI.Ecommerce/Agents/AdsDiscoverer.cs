using System.Text.Json;
using Niobium.AI.Ecommerce.AgentTools;
using Niobium.AI.Ecommerce.Contracts;

namespace Niobium.AI.Ecommerce.Agents
{
    internal class AdsDiscoverer(IMetaAdsLibrary adsLibrary) : IResponseAgent<AdsDiscovererInput, AdsDiscovererOutput>
    {
        public string Name => nameof(AdsDiscoverer);

        public async Task<AdsDiscovererOutput> GetResponseAsync(string conversationID, AdsDiscovererInput input, CancellationToken cancellationToken)
        {
            if (!Country.TryParse(input.Country, out var country))
            {
                throw new ArgumentException($"Invalid country: {input.Country}", nameof(input));
            }

            var ads = await adsLibrary.SearchAdsAsync(input.Keyword, country, cancellationToken: cancellationToken);
            var result = new AdsDiscovererOutput();
            result.AddRange(ads.SearchResults);
            return result;
        }

        public async Task<string> RunAsync(string conversationID, string input, CancellationToken cancellationToken)
        {
            var discovererInput = JsonSerializer.Deserialize<AdsDiscovererInput>(input, SerializationOptions.SnakeCase) ?? throw new ArgumentException("Failed to parse input", nameof(input));

            if (!Country.TryParse(discovererInput.Country, out var country))
            {
                throw new ArgumentException($"Invalid country: {discovererInput.Country}", nameof(input));
            }

            var ads = await adsLibrary.SearchAdsAsync(discovererInput.Keyword, country, cancellationToken: cancellationToken);
            var result = new AdsDiscovererOutput();
            result.AddRange(ads.SearchResults);
            return JsonSerializer.Serialize(result, SerializationOptions.SnakeCase);
        }
    }
}
