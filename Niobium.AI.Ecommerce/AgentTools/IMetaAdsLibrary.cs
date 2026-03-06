using Niobium.AI.Ecommerce.Contracts;

namespace Niobium.AI.Ecommerce.AgentTools
{
    public interface IMetaAdsLibrary
    {
        Task<MetaAdsSearchResponse> SearchAdsAsync(string keyword, Country country, DateOnly? activeSince = null, CancellationToken? cancellationToken = null);
    }
}
        