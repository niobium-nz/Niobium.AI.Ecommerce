using System.Diagnostics.CodeAnalysis;

namespace Niobium.AI.WebScraper.Firecrawl
{
    public class WebScrapeResult
    {
        public bool Success { get; set; }

        [MemberNotNull(nameof(Success))]
        public string? WebPageContentInMarkdownFormat { get; set; }

        public Dictionary<string, object> Metadata { get; init; } = [];
    }
}
