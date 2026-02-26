using Microsoft.AspNetCore.Mvc;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace Niobium.Ads.MCP
{
    [McpServerToolType]
    [ApiController]
    public class AdsLibraryTool(IMetaAdsLibrary adsLibrary)
    {
        [McpServerTool, Description("Search against ads Library for ads by keyword, country and active date.")]
        [HttpGet(Endpoints.SearchAds)]
        public async Task<List<MetaAd>> SearchAds(
            [Description("The keyword to search for.")] string keyword,
            [Description("The country to search for. Must specify one and can only specify one country. Provide country code in ISO 3166-1 alpha-2 format, such as 'US'")] string country,
            [Description("Optionally the date since when the ads have been active. Use ISO 8601 format: yyyy-MM-dd")] DateOnly? activeSince = null)
            => Country.TryParse(country, out var c)
                ? (await adsLibrary.SearchAdsAsync(keyword,  c, activeSince)).SearchResults
                : throw new ApplicationException("Invalid country code.");
    }
}
