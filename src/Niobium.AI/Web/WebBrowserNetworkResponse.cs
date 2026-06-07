namespace Niobium.AI.Web
{
    public sealed record WebBrowserNetworkResponse
    {
        public required IReadOnlyDictionary<string, string> Headers { get; init; }

        public required bool Ok { get; init; }

        public required int Status { get; init; }

        public required string StatusText { get; init; }

        public required string Url { get; init; }
    }
}
