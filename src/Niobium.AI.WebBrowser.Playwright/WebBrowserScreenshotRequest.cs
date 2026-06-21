namespace Niobium.AI.WebBrowser.Playwright
{
    public sealed record WebBrowserScreenshotRequest
    {
        public bool FullPage { get; init; }

        public string? Path { get; init; }

        public float? TimeoutMs { get; init; }
    }
}
