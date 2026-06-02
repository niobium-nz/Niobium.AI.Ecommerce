using Niobium.AI.Ecommerce.Contracts;
using Niobium.AI.Ecommerce.Tools;

namespace Niobium.AI.Ecommerce.Agents
{
    internal class AdsDiscoverer(IMetaAdsLibrary adsLibrary) : IResponseGenerator<AdsDiscovererInput, List<MetaAd>>
    {
        public string Id => nameof(AdsDiscoverer);

        public async Task<List<MetaAd>> RunAsync(AdsDiscovererInput input, CancellationToken? cancellationToken = null)
        {
            cancellationToken ??= CancellationToken.None;

            if (!Country.TryParse(input.Country, out Country country))
            {
                throw new ArgumentException($"Invalid country: {input.Country}", nameof(input));
            }

            MetaAdsSearchResponse response = await adsLibrary.SearchAdsAsync(input.Keyword, country, cancellationToken: cancellationToken);
            return response.SearchResults;
        }
    }
}
