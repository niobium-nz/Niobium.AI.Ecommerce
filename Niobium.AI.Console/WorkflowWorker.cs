using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Niobium.AI.Console
{
    internal class WorkflowWorker(IWorkflow workflow, ILogger<WorkflowWorker> logger) : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var rendering = workflow.Render();
            logger.LogInformation($"Running workflow: \n{rendering}");

            var conversationID = Guid.NewGuid();
            var input = """
                {
                    "BusinessName": "Mid-Class Community Restaurant and Bar",
                    "Location": "Morningside, Auckland, New Zealand",
                    "BusinessType": "Restaurant and Bar",
                    "ProductsSold": [
                        "Food",
                        "Alcoholic Beverages",
                        "Non-Alcoholic Beverages"
                    ],
                    "TypicalSpend": "$20-$50 per person",
                    "AdAccountId": "1995422867683456",
                    "CampaignName": "Followers",
                    "AdSetName": "Attractive Shorts"
                }
                """;
            var result = await workflow.RunAsync(conversationID.ToString(), input, cancellationToken);
            System.Console.WriteLine(result);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
