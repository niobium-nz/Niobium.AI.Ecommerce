using Niobium.AI.Shorts.Agents;
using Niobium.AI.Shorts.Contracts;

namespace Niobium.AI.Shorts.Workflows
{
    internal class AttractiveShortWorkflow(
        AttractiveShortProducer attractiveShortProducer,
        MetaVideoAdCreator metaVideoAdCreator)
        : IWorkflow
    {
        public async Task RunAsync(string conversationID, CancellationToken cancellationToken)
        {
            var video = await attractiveShortProducer.GetVideoAsync(
                    conversationID,
                    new AttractiveShortProducerInput
                    {
                        BusinessName = "Beach Front Restaurant and Cocktail Bar",
                        Location = "Mission Bay, Auckland, New Zealand",
                        BusinessType = "Beach Front Restaurant and Cocktail Bar",
                        TypicalSpend = "$60-$80"
                    },
                    cancellationToken);

            var result = await metaVideoAdCreator.GetResponseAsync(
                conversationID,
                new MetaVideoAdCreatorInput
                {
                    AdAccountId = "26137758852540494",
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
