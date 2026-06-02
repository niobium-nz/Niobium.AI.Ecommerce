using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Niobium.AI.Ecommerce.Contracts;

namespace Niobium.AI.Ecommerce.Tools
{
    internal class ScrapecreatorsMetaAdsLibrary(HttpClient httpClient) : IMetaAdsLibrary
    {
        private const string ApiEndpoint = "https://api.scrapecreators.com/v1/facebook/adLibrary/search/ads";

        public async Task<MetaAdsSearchResponse> SearchAdsAsync(string keyword, Country country, DateOnly? activeSince = null, CancellationToken? cancellationToken = null)
        {
            MetaAdsSearchResponse result = await this.SearchAdsAsync(keyword, country, activeSince, null);
            for (int i = 0; i < 3 && !String.IsNullOrWhiteSpace(result.Cursor); i++)
            {
                MetaAdsSearchResponse nextPageResult = await this.SearchAdsAsync(keyword, country, activeSince, result.Cursor, cancellationToken: cancellationToken);
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
            _ = cancellationToken ?? CancellationToken.None;

            string? apikey = Environment.GetEnvironmentVariable("SCRAPECREATORS_API_KEY");
            if (String.IsNullOrEmpty(apikey))
            {
                throw new InvalidOperationException("SCRAPECREATORS_API_KEY environment variable is not set.");
            }

            if (!activeSince.HasValue)
            {
                activeSince = DateOnly.FromDateTime(DateTime.UtcNow);
            }

            Dictionary<string, string?> queryParameters = new()
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

            if (!String.IsNullOrWhiteSpace(cursor))
            {
                queryParameters.Add("cursor", cursor);
            }

            UriBuilder uriBuilder = new(ApiEndpoint)
            {
                Query = QueryString.Create(queryParameters).ToString()
            };

            HttpRequestMessage request = new(HttpMethod.Get, uriBuilder.Uri)
            {
                Headers = { { "x-api-key", apikey } }
            };

            HttpResponseMessage response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"API request failed with status code {response.StatusCode}: {errorContent}");
            }

            string responseContent = await response.Content.ReadAsStringAsync();
            MetaAdsSearchResponse? result = null;
            if (!String.IsNullOrWhiteSpace(responseContent))
            {
                result = JsonSerializer.Deserialize<MetaAdsSearchResponse>(responseContent, SerializationOptions.SnakeCase);
            }

            return result ?? new MetaAdsSearchResponse
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
