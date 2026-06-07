namespace Niobium.AI.Web
{
    public sealed record WebBrowserFormFieldInput
    {
        public required string Selector { get; init; }

        public string? Value { get; init; }
    }
}
