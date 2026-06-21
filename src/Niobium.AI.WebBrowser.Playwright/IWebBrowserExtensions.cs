using Microsoft.Extensions.AI;

namespace Niobium.AI.WebBrowser.Playwright
{
    public static class IWebBrowserExtensions
    {
        public static IEnumerable<AITool> AsAITools(this IWebBrowser browser)
        {
            ArgumentNullException.ThrowIfNull(browser);

            return
            [
                AIFunctionFactory.Create(browser.OpenTabAsync, "browser_new_tab", "Open a new browser tab in the current browser session and return the created tab information.", SerializationOptions.SnakeCase),
                AIFunctionFactory.Create(browser.CloseTabAsync, "browser_close_tab", "Close an existing browser tab by its tab id.", SerializationOptions.SnakeCase),
                AIFunctionFactory.Create(browser.NavigateAsync, "browser_navigate", "Navigate a browser tab to the provided URL and return the resulting navigation information.", SerializationOptions.SnakeCase),
                AIFunctionFactory.Create(browser.SnapshotAsync, "browser_snapshot", "Capture a detailed snapshot of the current page including HTML and structured DOM information.", SerializationOptions.SnakeCase),
                AIFunctionFactory.Create(browser.WaitForAsync, "browser_wait_for", "Wait in a browser tab for a selector, page load state, or timeout based on the provided request.", SerializationOptions.SnakeCase),
                AIFunctionFactory.Create(browser.ClickAsync, "browser_click", "Click an element in a browser tab using the provided selector.", SerializationOptions.SnakeCase),
                AIFunctionFactory.Create(browser.FillFormAsync, "browser_fill_form", "Fill multiple form fields in a browser tab using a list of selector and value inputs.", SerializationOptions.SnakeCase),
                AIFunctionFactory.Create(browser.TypeAsync, "browser_type", "Type text into a browser element identified by the provided selector.", SerializationOptions.SnakeCase),
                AIFunctionFactory.Create(browser.SelectOptionAsync, "browser_select_option", "Select one or more option values in a browser select element identified by the provided selector.", SerializationOptions.SnakeCase),
                AIFunctionFactory.Create(browser.TakeScreenshotAsync, "browser_take_screenshot", "Take a screenshot of a browser tab and return the saved file information.", SerializationOptions.SnakeCase),
                AIFunctionFactory.Create(browser.SetDialogHandlerAsync, "browser_handle_dialog", "Configure how browser dialogs should be accepted or dismissed for a browser tab.", SerializationOptions.SnakeCase),
                AIFunctionFactory.Create(browser.GetTabsAsync, "browser_tabs", "List all currently open browser tabs in the current browser session.", SerializationOptions.SnakeCase),
                AIFunctionFactory.Create(browser.GetConsoleMessagesAsync, "browser_console_messages", "Get console messages captured for a browser tab.", SerializationOptions.SnakeCase),
                AIFunctionFactory.Create(browser.GetNetworkRequestsAsync, "browser_network_requests", "Get network requests and response details captured for a browser tab.", SerializationOptions.SnakeCase),
                AIFunctionFactory.Create(browser.EvaluateAsync, "browser_evaluate", "Evaluate a JavaScript expression in the context of a browser tab and return the JSON result.", SerializationOptions.SnakeCase),
                AIFunctionFactory.Create(browser.RunCodeAsync, "browser_run_code", "Run JavaScript code in the context of a browser tab and return the JSON result.", SerializationOptions.SnakeCase),
            ];
        }
    }
}
