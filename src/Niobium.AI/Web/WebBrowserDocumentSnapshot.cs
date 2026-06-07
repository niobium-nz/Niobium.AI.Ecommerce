namespace Niobium.AI.Web
{
    public sealed record WebBrowserDocumentSnapshot
    {
        public string? DocumentElement { get; init; }

        public required IReadOnlyList<WebBrowserFormSnapshot> Forms { get; init; }

        public required IReadOnlyList<WebBrowserLinkSnapshot> Links { get; init; }

        public string? BodyText { get; init; }
    }
}
