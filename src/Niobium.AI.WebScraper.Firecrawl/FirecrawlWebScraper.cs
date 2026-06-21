using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Niobium.AI.WebScraper.Firecrawl
{
    internal class FirecrawlWebScraper(HttpClient httpClient, IOptions<FirecrawlOptions> options, ILogger<FirecrawlWebScraper> logger) : IWebScraper
    {
        public async Task<WebScrapeResult> ScrapeAsync(Uri uri, CancellationToken? cancellationToken = null)
        {
            HttpRequestMessage request = new(HttpMethod.Post, "https://api.firecrawl.dev/v2/scrape");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.Value.ApiKey);
            request.Content = JsonContent.Create(new FirecrawlScrapeRequest(uri.AbsoluteUri, ["markdown"], false), mediaType: new MediaTypeHeaderValue("application/json"), options: JsonSerializerOptions.Web);

            HttpResponseMessage response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Firecrawl API request failed with status code {response.StatusCode}: {errorContent}");
            }

            FirecrawlScrapeResponse? responseContent = await response.Content.ReadFromJsonAsync<FirecrawlScrapeResponse>();
            if (responseContent == null || !responseContent.Success)
            {
                logger.LogError($"Firecrawl API request failed: {responseContent}");
                return new WebScrapeResult
                {
                    Success = false
                };
            }

            return new WebScrapeResult
            {
                Success = true,
                WebPageContentInMarkdownFormat = responseContent.Data.Markdown,
                Metadata = responseContent.Data.Metadata
            };
        }

        internal record FirecrawlScrapeRequest(string Url, string[] Formats, bool OnlyMainContent);
        internal record FirecrawlScrapeResponse(bool Success, FirecrawlScrapeResponseData Data);
        internal record FirecrawlScrapeResponseData(string Markdown, Dictionary<string, object> Metadata);
    }
}
