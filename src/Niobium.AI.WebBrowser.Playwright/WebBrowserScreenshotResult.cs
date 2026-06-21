namespace Niobium.AI.WebBrowser.Playwright
{
    public sealed record WebBrowserScreenshotResult
    {
        public required WebBrowserTabInfo Tab { get; init; }

        public required string Path { get; init; }

        public required Uri Uri { get; init; }
    }
}
