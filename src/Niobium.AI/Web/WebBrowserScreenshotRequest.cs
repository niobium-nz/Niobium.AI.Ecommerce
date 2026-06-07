namespace Niobium.AI.Web
{
    public sealed record WebBrowserScreenshotRequest
    {
        public bool FullPage { get; init; }

        public string? Path { get; init; }

        public float? TimeoutMs { get; init; }
    }
}
