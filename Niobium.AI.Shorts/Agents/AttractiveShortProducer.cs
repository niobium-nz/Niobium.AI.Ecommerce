using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Niobium.AI.Shorts.Contracts;
using Niobium.AI.Shorts.Skills;

namespace Niobium.AI.Shorts.Agents
{
    internal class AttractiveShortProducer(
        IFileStorage fileStorage,
        IVideoClientFactory videoClientFactory,
        IChatClientFactory chatClientFactory,
        ILogger<AttractiveShortProducer> logger)
            : GenericVideoAIAgent<AttractiveShortProducerInput, AttractiveShortProducerOutput>(fileStorage, videoClientFactory, chatClientFactory, logger)
    {
        public override string Name => nameof(AttractiveShortProducer);

        protected override Type InstructionsResourceBaseType => this.GetType();

        protected override ReasoningEffort Reasoning => ReasoningEffort.Medium;

        protected override async Task OnVideoGotAsync(string conversationID, AttractiveShortProducerInput input, AttractiveShortProducerOutput output, Stream videoStream, CancellationToken cancellationToken)
        {
            using (videoStream)
            {
                Stream videoStreamWithSubtitle = await BurnSubtitleToVideo.BurnInSubtitlesAsync(videoStream, output, cancellationToken);
                await base.OnVideoGotAsync(conversationID, input, output, videoStreamWithSubtitle, cancellationToken);
            }
        }
    }
}
