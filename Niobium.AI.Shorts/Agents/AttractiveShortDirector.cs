using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Niobium.AI.Shorts.Contracts;

namespace Niobium.AI.Shorts.Agents
{
    internal class AttractiveShortDirector(IChatClientFactory clientFactory, ILogger<AttractiveShortDirector> logger)
        : TypedGenericLanguageAIAgent<AttractiveShortDirectorInput, AttractiveShortDirectorOutput>(clientFactory, logger)
    {
        public override string Name => nameof(AttractiveShortDirector);

        protected override Type InstructionsResourceBaseType => this.GetType();

        protected override ReasoningEffort Reasoning => ReasoningEffort.Medium;
    }
}
