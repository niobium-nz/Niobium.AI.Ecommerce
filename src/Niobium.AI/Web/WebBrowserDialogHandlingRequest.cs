namespace Niobium.AI.Web
{
    public sealed record WebBrowserDialogHandlingRequest
    {
        public bool Accept { get; init; } = true;

        public string? PromptText { get; init; }
    }
}
