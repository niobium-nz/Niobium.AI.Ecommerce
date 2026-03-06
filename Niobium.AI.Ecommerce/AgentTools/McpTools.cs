using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

namespace Niobium.AI.Ecommerce.AgentTools
{
    internal class McpTools(AdsLibraryTool adsLibraryTool)
    {
        private static IEnumerable<McpClientTool>? playwrightTools;

        public async Task<IEnumerable<AITool>> GetPlaywrightToolsAsync(CancellationToken cancellationToken)
        {
            if (playwrightTools == null)
            {
                var playwrightMcpClient = await McpClient.CreateAsync(new StdioClientTransport(new()
                {
                    Name = "Playwright",
                    Command = "npx",
                    Arguments = ["-y", "@playwright/mcp@latest", "--extension"],
                }), cancellationToken: cancellationToken);

                playwrightTools = await playwrightMcpClient.ListToolsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            return playwrightTools.Cast<AITool>();
        }

        public Task<IEnumerable<AITool>> GetAdsLibraryToolsAsync(CancellationToken cancellationToken)
            => Task.FromResult<IEnumerable<AITool>>([AIFunctionFactory.Create(adsLibraryTool.SearchAds)]);

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
