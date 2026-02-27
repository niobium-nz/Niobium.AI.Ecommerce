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
    }
}
