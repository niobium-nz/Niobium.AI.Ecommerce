using Azure.AI.Projects.OpenAI;
using OpenAI.Responses;

namespace Niobium.Ads.Analyst
{
    internal class McpTools
    {
        public static ResponseTool AdsLibraryMcpTool
        {
            get
            {
                var tool = ResponseTool.CreateMcpTool(
                    serverLabel: "adslibrary",
                    serverUri: new Uri("https://niobiumadsmcpapp.mangosky-a7b92dc1.westus2.azurecontainerapps.io/"),
                    toolCallApprovalPolicy: new McpToolCallApprovalPolicy(GlobalMcpToolCallApprovalPolicy.NeverRequireApproval));
                tool.ProjectConnectionId = "adslibrary";
                return tool;
            }
        }

        public static ResponseTool PlayWrightMcpTool
        {
            get
            {
                var tool = ResponseTool.CreateMcpTool(
                    serverLabel: "playwright",
                    serverUri: new Uri("http://minecraft.5he11.com:8931/sse"),
                    toolCallApprovalPolicy: new McpToolCallApprovalPolicy(GlobalMcpToolCallApprovalPolicy.NeverRequireApproval));
                tool.ProjectConnectionId = "playwright";
                return tool;
            }
        }


        static async Task CleanupPlaywrightTabsAsync(IList<McpClientTool> mcpTools)
        {
            ArgumentNullException.ThrowIfNull(mcpTools);

            McpClientTool? tabsTool = mcpTools.FirstOrDefault(t => string.Equals(t.Name, "browser_tabs", StringComparison.Ordinal));
            McpClientTool? closeTool = mcpTools.FirstOrDefault(t => string.Equals(t.Name, "browser_close", StringComparison.Ordinal));

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
                        await tabsTool.InvokeAsync(new AIFunctionArguments
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

                await closeTool.InvokeAsync().ConfigureAwait(false);
            }
            catch
            {
                // Best-effort cleanup; ignore failures.
            }
        }

    }
}
