namespace Niobium.AI.WebBrowser.Playwright
{
    public sealed record WebBrowserWaitForRequest
    {
        public WebBrowserWaitTarget Target { get; init; } = WebBrowserWaitTarget.Selector;

        public string? Value { get; init; }

        public WebBrowserWaitForSelectorState? State { get; init; }

        public WebBrowserLoadState? LoadState { get; init; }

        public float TimeoutMs { get; init; } = 3000;
    }
}
