using Microsoft.Extensions.AI;

namespace Niobium.AI.WebScraper.Firecrawl
{
    public static class IWebScraperExtensions
    {
        public static IEnumerable<AITool> AsAITools(this IWebScraper webScraper)
        {
            ArgumentNullException.ThrowIfNull(webScraper);

            return
            [
                AIFunctionFactory.Create(webScraper.ScrapeAsync, "scrape_web_page", "Scrape a specified web page and converts its content into markdown format.", SerializationOptions.SnakeCase),
            ];
        }
    }
}
