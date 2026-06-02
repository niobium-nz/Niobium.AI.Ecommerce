using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

namespace Niobium.AI
{
    public class McpTools
    {
        private static readonly string[] PlaywrightToolNames =
        {
            "browser_navigate",
            "browser_snapshot",
            "browser_wait_for",
            "browser_click",
            "browser_fill_form",
            "browser_type",
            "browser_select_option",
            "browser_take_screenshot",
            "browser_handle_dialog",
            "browser_tabs",
            "browser_console_messages",
            "browser_network_requests",
            "browser_evaluate",
            "browser_run_code",
        };
        private static IEnumerable<McpClientTool>? playwrightTools;

        public IEnumerable<AITool> GetWebSearchTools() => [new HostedWebSearchTool()];

        public IEnumerable<AITool> GetCodeInterpreterTools() => [new HostedCodeInterpreterTool()];
        
        public IEnumerable<AITool> GetPlaywrightTools()
        {
            if (playwrightTools == null)
            {
                McpClient playwrightMcpClient = McpClient.CreateAsync(new StdioClientTransport(new()
                {
                    Name = "Playwright",
                    Command = "npx",
                    Arguments = ["-y", "@playwright/mcp@latest", "--extension"],
                    EnvironmentVariables = new Dictionary<string, string?>
                    {
                        ["PLAYWRIGHT_MCP_EXTENSION_TOKEN"] = Environment.GetEnvironmentVariable("PLAYWRIGHT_MCP_EXTENSION_TOKEN")
                    }
                })).GetAwaiter().GetResult();

                playwrightTools = playwrightMcpClient.ListToolsAsync().GetAwaiter().GetResult();
            }

            return playwrightTools.Cast<AITool>().Where(t => PlaywrightToolNames.Contains(t.Name));
        }

        public async Task CleanupPlaywrightTabsAsync(CancellationToken cancellationToken)
        {
            if (playwrightTools == null)
            {
                return;
            }

            McpClientTool? tabsTool = playwrightTools.FirstOrDefault(t => String.Equals(t.Name, "browser_tabs", StringComparison.Ordinal));
            McpClientTool? closeTool = playwrightTools.FirstOrDefault(t => String.Equals(t.Name, "browser_close", StringComparison.Ordinal));

            try
            {
                if (tabsTool is null)
                {
                    return;
                }

                //Best - effort: close unknown extra tabs without relying on `action: list` output format.
                for (int i = 50; i >= 0; i--)
                {
                    try
                    {
                        _ = await tabsTool.InvokeAsync(new AIFunctionArguments
                        {
                            ["action"] = "close",
                            ["index"] = i
                        }).ConfigureAwait(false);
                    }
                    catch
                    {
                        //Ignore per-tab failures; some indices may not exist.
                    }
                }
            }
            catch
            {
                //Best - effort cleanup; ignore failures.
            }

            try
            {
                if (closeTool is null)
                {
                    return;
                }

                _ = await closeTool.InvokeAsync().ConfigureAwait(false);
            }
            catch
            {
                //Best - effort cleanup; ignore failures.
            }
        }

    }
}
