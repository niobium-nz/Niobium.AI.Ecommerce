namespace Niobium.AI.WebBrowser.Playwright
{
    public sealed record WebBrowserNetworkRequest
    {
        public required IReadOnlyDictionary<string, string> Headers { get; init; }

        public required bool IsNavigationRequest { get; init; }

        public required string Method { get; init; }

        public string? RequestBody { get; init; }

        public required string ResourceType { get; init; }

        public WebBrowserNetworkResponse? Response { get; set; }

        public required string Url { get; init; }
    }
}
