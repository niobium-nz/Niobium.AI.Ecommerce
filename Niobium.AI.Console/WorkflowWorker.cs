using Microsoft.Extensions.Hosting;

namespace Niobium.AI.Console
{
    internal class WorkflowWorker(IWorkflow workflow) : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var conversationID = Guid.NewGuid();
            await workflow.RunAsync(conversationID.ToString(), cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
