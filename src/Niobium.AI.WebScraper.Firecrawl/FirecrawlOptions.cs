namespace Niobium.AI.WebScraper.Firecrawl
{
    public class FirecrawlOptions
    {
        public static readonly string SectionName = nameof(FirecrawlOptions).ToUpperInvariant();

        public required string ApiKey { get; set; }
    }
}
