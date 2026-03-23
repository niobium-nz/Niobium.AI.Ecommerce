using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Niobium.AI.Shorts.Contracts;

namespace Niobium.AI.Shorts.Agents
{
    internal class AttractiveShortScreenwriter(
        IChatClientFactory chatClientFactory,
        ILogger<AttractiveShortScreenwriter> logger)
            : TypedResponseAgent<AttractiveShortScreenwriterInput, AttractiveShortScreenwriterOutput>(chatClientFactory, logger)
    {
        public override string Id => nameof(AttractiveShortScreenwriter);

        protected override Type InstructionsResourceBaseType => this.GetType();

        protected override ReasoningEffort Reasoning => ReasoningEffort.Medium;
    }
}
