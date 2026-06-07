namespace Niobium.AI.Web
{
    public sealed record WebBrowserLinkSnapshot
    {
        public string? Href { get; init; }

        public required int Index { get; init; }

        public string? Text { get; init; }

        public string? Title { get; init; }
    }
}
