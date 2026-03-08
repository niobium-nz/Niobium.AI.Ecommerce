using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Niobium.AI.Shorts.Contracts;

namespace Niobium.AI.Shorts.Agents
{
    internal class MetaVideoAdPublisher(IChatClientFactory clientFactory, ILogger<MetaVideoAdPublisher> logger)
        : TypedGenericLanguageAIAgent<MetaVideoAdPublisherInput, MetaVideoAdPublisherOutput>(clientFactory, logger)
    {
        public override string Name => nameof(MetaVideoAdPublisher);

        protected override Type InstructionsResourceBaseType => this.GetType();

        protected override ReasoningEffort Reasoning => ReasoningEffort.High;
    }
}
