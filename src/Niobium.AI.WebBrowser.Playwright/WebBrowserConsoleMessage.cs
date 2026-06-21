namespace Niobium.AI.WebBrowser.Playwright
{
    public sealed record WebBrowserConsoleMessage
    {
        public required string Text { get; init; }

        public required string Type { get; init; }
    }
}
