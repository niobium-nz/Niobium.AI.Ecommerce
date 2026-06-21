namespace Niobium.AI.WebBrowser.Playwright
{
    public sealed record WebBrowserNavigationResult
    {
        public required WebBrowserTabInfo Tab { get; init; }

        public string? Url { get; init; }
    }
}
