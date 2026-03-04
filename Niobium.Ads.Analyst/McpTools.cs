using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using Niobium.Ads.Analyst.AgentTools;

namespace Niobium.Ads.Analyst
{
    internal class McpTools(AdsLibraryTool adsLibraryTool)
    {
        private static McpClient? playwrightMcpClient;

        public async Task<IEnumerable<AITool>> GetPlaywrightToolsAsync(CancellationToken cancellationToken)
        {
            playwrightMcpClient ??= await McpClient.CreateAsync(new StdioClientTransport(new()
            {
                Name = "Playwright",
                Command = "npx",
                Arguments = ["-y", "@playwright/mcp@latest", "--extension"],
            }), cancellationToken: cancellationToken);

            IList<McpClientTool> mcpTools = await playwrightMcpClient.ListToolsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            return mcpTools.Cast<AITool>();
        }

        public Task<IEnumerable<AITool>> GetAdsLibraryToolsAsync(CancellationToken cancellationToken)
            => Task.FromResult<IEnumerable<AITool>>([AIFunctionFactory.Create(adsLibraryTool.SearchAds)]);

        private static async Task CleanupPlaywrightTabsAsync(IList<McpClientTool> mcpTools)
        {
            ArgumentNullException.ThrowIfNull(mcpTools);

            McpClientTool? tabsTool = mcpTools.FirstOrDefault(t => String.Equals(t.Name, "browser_tabs", StringComparison.Ordinal));
            McpClientTool? closeTool = mcpTools.FirstOrDefault(t => String.Equals(t.Name, "browser_close", StringComparison.Ordinal));

            try
            {
                if (tabsTool is null)
                {
                    return;
                }

                // Best-effort: close unknown extra tabs without relying on `action: list` output format.
                for (int i = 50; i >= 1; i--)
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
                        // Ignore per-tab failures; some indices may not exist.
                    }
                }
            }
            catch
            {
                // Best-effort cleanup; ignore failures.
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
                // Best-effort cleanup; ignore failures.
            }
        }

    }
}
