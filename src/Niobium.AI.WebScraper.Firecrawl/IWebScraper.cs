namespace Niobium.AI.WebScraper.Firecrawl
{
    public interface IWebScraper
    {
        Task<WebScrapeResult> ScrapeAsync(Uri uri, CancellationToken? cancellationToken = null);
    }
}
