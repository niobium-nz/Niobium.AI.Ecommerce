namespace Niobium.AI.WebBrowser.Playwright
{
    public sealed record WebBrowserTabInfo
    {
        public required int Id { get; init; }

        public string? Title { get; init; }

        public string? Url { get; init; }
    }
}
