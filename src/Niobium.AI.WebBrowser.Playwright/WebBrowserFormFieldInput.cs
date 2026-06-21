namespace Niobium.AI.WebBrowser.Playwright
{
    public sealed record WebBrowserFormFieldInput
    {
        public required string Selector { get; init; }

        public string? Value { get; init; }
    }
}
