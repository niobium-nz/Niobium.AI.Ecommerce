namespace Niobium.AI.Web
{
    public sealed record WebBrowserFormSnapshot
    {
        public string? Action { get; init; }

        public required IReadOnlyList<WebBrowserFormFieldSnapshot> Fields { get; init; }

        public string? Id { get; init; }

        public required int Index { get; init; }

        public string? Method { get; init; }

        public string? Name { get; init; }
    }
}
