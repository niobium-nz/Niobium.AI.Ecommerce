using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Niobium.Ads.Analyst.AgentTools
{
    internal class ScrapecreatorsMetaAdsLibrary(HttpClient httpClient) : IMetaAdsLibrary
    {
        private const string ApiEndpoint = "https://api.scrapecreators.com/v1/facebook/adLibrary/search/ads";

        public async Task<MetaAdsSearchResponse> SearchAdsAsync(string keyword, Country country, DateOnly? activeSince = null, CancellationToken? cancellationToken = null)
        {
            var result = await SearchAdsAsync(keyword, country, activeSince, null);
            for (int i = 0; i < 3 && !string.IsNullOrWhiteSpace(result.Cursor); i++)
            {
                var nextPageResult = await SearchAdsAsync(keyword, country, activeSince, result.Cursor, cancellationToken: cancellationToken);
                if (nextPageResult.SearchResultsCount > 0)
                {
                    result.SearchResults.AddRange(nextPageResult.SearchResults);
                    result.SearchResultsCount += nextPageResult.SearchResultsCount;
                }
                else
                {
                    break;
                }
            }
            return result;
        }

        private async Task<MetaAdsSearchResponse> SearchAdsAsync(string keyword, Country country, DateOnly? activeSince = null, string? cursor = null, CancellationToken? cancellationToken = null)
        {
            cancellationToken ??= CancellationToken.None;

            var apikey = Environment.GetEnvironmentVariable("SCRAPECREATORS_API_KEY");
            if (string.IsNullOrEmpty(apikey))
            {
                throw new InvalidOperationException("SCRAPECREATORS_API_KEY environment variable is not set.");
            }

            if (!activeSince.HasValue)
            {
                activeSince = DateOnly.FromDateTime(DateTime.UtcNow);
            }

            var queryParameters = new Dictionary<string, string?>
            {
                { "query", keyword },
                { "sort_by", "total_impressions" },
                { "search_type", "keyword_exact_phrase" },
                { "ad_type", "all" },
                { "country", country.Alpha2 },
                { "status", "ACTIVE" },
                { "media_type", "ALL" },
                { "start_date", $"{activeSince.Value:yyyy-MM-dd}" },
            };

            if (!string.IsNullOrWhiteSpace(cursor))
            {
                queryParameters.Add("cursor", cursor);
            }

            var uriBuilder = new UriBuilder(ApiEndpoint)
            {
                Query = QueryString.Create(queryParameters).ToString()
            };

            var request = new HttpRequestMessage(HttpMethod.Get, uriBuilder.Uri)
            {
                Headers = { { "x-api-key", apikey } }
            };

            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"API request failed with status code {response.StatusCode}: {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            MetaAdsSearchResponse? result = null;
            if (!string.IsNullOrWhiteSpace(responseContent))
            {
                result = JsonSerializer.Deserialize<MetaAdsSearchResponse>(responseContent, SerializationOptions.SnakeCase);
            }

            return result ??= new MetaAdsSearchResponse
            {
                Success = false,
                CreditsRemaining = 999,
                SearchResultsCount = 0,
                Cursor = null,
                SearchResults = []
            };
        }
    }
}
