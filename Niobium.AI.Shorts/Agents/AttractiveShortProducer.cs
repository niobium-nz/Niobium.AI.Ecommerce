using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Niobium.AI.Shorts.Contracts;

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
    }
}
