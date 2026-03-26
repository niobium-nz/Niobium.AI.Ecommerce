using Azure.Data.Tables;
using Microsoft.Agents.AI.Workflows;
using Niobium.AI.Shorts.Contracts;

namespace Niobium.AI.Shorts.Executors
{
    internal class MetaVideoAdCreatorAdaptor(TableServiceClient tableServiceClient) : Executor<Uri, MetaVideoAdCreatorInput>(nameof(MetaVideoAdCreatorAdaptor))
    {
        public override async ValueTask<MetaVideoAdCreatorInput> HandleAsync(Uri message, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            var userInput = await context.ReadStateAsync<AttractiveShortWorkflowInput>(States.UserInput, scopeName: States.SharedScope, cancellationToken: cancellationToken);
            var videoInstruction = await context.ReadStateAsync<AttractiveShortScreenwriterOutput>(States.VideoInstructions, scopeName: States.SharedScope, cancellationToken: cancellationToken);

            if (videoInstruction == null || userInput == null)
            {
                throw new InvalidOperationException($"Invalid workflow state: either {States.UserInput} or {States.VideoInstructions} not found.");
            }

            var tableClient = tableServiceClient.GetTableClient("VideoIdea");
            _ = await tableClient.AddEntityAsync(new TableEntity(userInput.BusinessName, DateTimeOffset.UtcNow.ToReverseUnixTimestamp())
                    {
                        { "Value", videoInstruction.VideoIdea },
                        { "Prompt", videoInstruction.VideoPrompt },
                    }, cancellationToken);


            return new MetaVideoAdCreatorInput
            {
                AdAccountId = userInput.AdAccountId,
                CampaignName = userInput.CampaignName,
                AdSetName = userInput.AdSetName,
                VideoUrl = message.ToString(),
                PrimaryText = $"{videoInstruction.SocialPost}\n\n{String.Join(' ', videoInstruction.SocialPostTags)}"
            };
        }
    }
}
