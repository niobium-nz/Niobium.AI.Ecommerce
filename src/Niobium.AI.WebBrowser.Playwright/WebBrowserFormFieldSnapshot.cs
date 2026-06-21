namespace Niobium.AI.WebBrowser.Playwright
{
    public sealed record WebBrowserFormFieldSnapshot
    {
        public string? Id { get; init; }

        public required int Index { get; init; }

        public string? Name { get; init; }

        public string? TagName { get; init; }

        public string? Type { get; init; }

        public string? Value { get; init; }
    }
}
