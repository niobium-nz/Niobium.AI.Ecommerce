using Azure.Data.Tables;
using Microsoft.Agents.AI.Workflows;
using Niobium.AI.Shorts.Contracts;

namespace Niobium.AI.Shorts.Executors
{
    internal class AttractiveShortScreenwriterAdaptor(TableServiceClient tableServiceClient) : Executor<AttractiveShortWorkflowInput, AttractiveShortScreenwriterInput>(nameof(AttractiveShortScreenwriterAdaptor))
    {
        public override async ValueTask<AttractiveShortScreenwriterInput> HandleAsync(AttractiveShortWorkflowInput message, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            TableClient tableClient = tableServiceClient.GetTableClient("VideoIdea");
            List<TableEntity> ideas = await tableClient.QueryAsync<TableEntity>(x => x.PartitionKey == message.BusinessName, cancellationToken: cancellationToken).ToListAsync(cancellationToken: cancellationToken);
            return new AttractiveShortScreenwriterInput
            {
                BusinessName = message.BusinessName,
                Location = message.Location,
                BusinessType = message.BusinessType,
                TypicalSpend = message.TypicalSpend,
                PreviousVideoIdeas = [.. ideas.Select(x => x.GetString("Value"))],
                ProductsSold = message.ProductsSold
            };
        }
    }
}
