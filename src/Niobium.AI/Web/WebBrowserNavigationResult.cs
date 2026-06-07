namespace Niobium.AI.Web
{
    public sealed record WebBrowserNavigationResult
    {
        public required WebBrowserTabInfo Tab { get; init; }

        public string? Url { get; init; }
    }
}
