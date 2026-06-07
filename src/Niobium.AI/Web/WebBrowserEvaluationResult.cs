namespace Niobium.AI.Web
{
    public sealed record WebBrowserEvaluationResult
    {
        public bool IsError { get; init; }

        public required WebBrowserTabInfo Tab { get; init; }

        public required string JsonResult { get; init; }
    }
}
