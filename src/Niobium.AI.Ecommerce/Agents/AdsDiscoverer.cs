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

            DateTime stableAdsDate = DateTime.Now.AddMonths(-3);
            DateOnly stableAdsDateOnly = new(stableAdsDate.Year, stableAdsDate.Month, stableAdsDate.Day);
            MetaAdsSearchResponse response = await adsLibrary.SearchAdsAsync(input.Keyword, country, activeSince: stableAdsDateOnly, cancellationToken: cancellationToken);
            return response.SearchResults;
        }
    }
}
