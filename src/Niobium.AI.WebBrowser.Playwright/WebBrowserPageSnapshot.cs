namespace Niobium.AI.WebBrowser.Playwright
{
    public sealed record WebBrowserPageSnapshot
    {
        public required WebBrowserTabInfo Tab { get; init; }

        public string? Title { get; init; }

        public string? Url { get; init; }

        public required string Html { get; init; }

        public required WebBrowserDocumentSnapshot Document { get; init; }
    }
}
