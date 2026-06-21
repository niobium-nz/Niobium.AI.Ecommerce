namespace Niobium.AI.WebBrowser.Playwright
{
    public interface IWebBrowser : IAsyncDisposable, IDisposable
    {
        Task<WebBrowserTabInfo> OpenTabAsync(CancellationToken cancellationToken = default);

        Task CloseTabAsync(int tabId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<WebBrowserTabInfo>> GetTabsAsync(CancellationToken cancellationToken = default);

        Task<WebBrowserNavigationResult> NavigateAsync(int tabId, string url, CancellationToken cancellationToken = default);

        Task<WebBrowserPageSnapshot> SnapshotAsync(int tabId, CancellationToken cancellationToken = default);

        Task WaitForAsync(int tabId, WebBrowserWaitForRequest waitCondition, CancellationToken cancellationToken = default);

        Task ClickAsync(int tabId, string selector, CancellationToken cancellationToken = default);

        Task FillFormAsync(int tabId, IReadOnlyList<WebBrowserFormFieldInput> formFields, CancellationToken cancellationToken = default);

        Task TypeAsync(int tabId, string selector, string text, CancellationToken cancellationToken = default);

        Task SelectOptionAsync(int tabId, string selector, IReadOnlyList<string> optionValues, CancellationToken cancellationToken = default);

        Task<WebBrowserScreenshotResult> TakeScreenshotAsync(int tabId, WebBrowserScreenshotRequest screenshotRequest, CancellationToken cancellationToken = default);

        Task SetDialogHandlerAsync(int tabId, WebBrowserDialogHandlingRequest dialogHandling, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<WebBrowserConsoleMessage>> GetConsoleMessagesAsync(int tabId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<WebBrowserNetworkRequest>> GetNetworkRequestsAsync(int tabId, CancellationToken cancellationToken = default);

        Task<WebBrowserEvaluationResult> EvaluateAsync(int tabId, string javaScriptExpression, CancellationToken cancellationToken = default);

        Task<WebBrowserEvaluationResult> RunCodeAsync(int tabId, string javaScriptCode, CancellationToken cancellationToken = default);
    }
}
