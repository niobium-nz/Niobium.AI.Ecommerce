namespace Niobium.Ads.Analyst.AgentTools
{
    public interface IMetaAdsLibrary
    {
        Task<MetaAdsSearchResponse> SearchAdsAsync(string keyword, Country country, DateOnly? activeSince = null, CancellationToken? cancellationToken = null);
    }
}
        