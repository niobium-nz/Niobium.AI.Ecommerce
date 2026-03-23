using Niobium.AI.Shorts.Contracts;
using Niobium.AI.Shorts.Skills;

namespace Niobium.AI.Shorts.Agents
{
    internal class AttractiveShortProducer(IVideoClientFactory videoClientFactory)
        : Sora2VideoAgent<AttractiveShortScreenwriterOutput>(videoClientFactory)
    {
        public override string Id => nameof(AttractiveShortProducer);

        protected override async Task<Stream> OnResponseGotAsync(string conversationID, AttractiveShortScreenwriterOutput input, Stream videoStream, CancellationToken cancellationToken)
        {
            using (videoStream)
            {
                Stream videoStreamWithSubtitle = await BurnSubtitleToVideo.BurnInSubtitlesAsync(videoStream, input, input.SubtitlePlan, cancellationToken);
                return await base.OnResponseGotAsync(conversationID, input, videoStreamWithSubtitle, cancellationToken);
            }
        }
    }
}
