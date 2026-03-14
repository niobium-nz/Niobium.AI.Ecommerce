using Azure.Data.Tables;
using Niobium.AI.Shorts.Agents;
using Niobium.AI.Shorts.Contracts;

namespace Niobium.AI.Shorts.Workflows
{
    internal class AttractiveShortWorkflow(
        TableServiceClient tableServiceClient,
        AttractiveShortProducer attractiveShortProducer,
        MetaVideoAdCreator metaVideoAdCreator)
        : IWorkflow
    {
        public async Task RunAsync(string conversationID, CancellationToken cancellationToken)
        {
            //for (int i = 0; i < 5; i++)
            {
                var businessName = "Mid-class community Restaurant and Bar";
                var tableClient = tableServiceClient.GetTableClient("VideoIdea");
                var ideas = await tableClient.QueryAsync<TableEntity>(x => x.PartitionKey == businessName, cancellationToken: cancellationToken).ToListAsync();
                var video = await attractiveShortProducer.GetVideoAsync(
                    conversationID,
                    new AttractiveShortProducerInput
                    {
                        BusinessName = businessName,
                        Location = "Morningside, Auckland, New Zealand",
                        BusinessType = "Restaurant & Bar",
                        TypicalSpend = "$60-$80",
                        PreviousVideoIdeas = [.. ideas.Select(x => x.GetString("Value")!)],
                    },
                    cancellationToken);

                await tableClient.AddEntityAsync(new TableEntity(businessName, DateTimeOffset.UtcNow.ToReverseUnixTimestamp()) { { "Value", video.VideoIdea } }, cancellationToken);

                var result = await metaVideoAdCreator.GetResponseAsync(
                    conversationID,
                    new MetaVideoAdCreatorInput
                    {
                        AdAccountId = "1560340895219737",
                        CampaignName = "Followers",
                        AdSetName = "Attractive Shorts",
                        VideoUrl = video.VideoUrl!.ToString(),
                        PrimaryText = $"{video.SocialPost}\n\n{String.Join(' ', video.SocialPostTags)}"
                    },
                    cancellationToken);

                Console.WriteLine(result);
            }
        }
    }
}
