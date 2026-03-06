using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Niobium.AI.Shorts.Contracts;
using OpenAI;

namespace Niobium.AI.Shorts.Agents
{
    internal class AttractiveShortDirector(OpenAIClient client, ILogger<AttractiveShortDirector> logger)
        : GenericResponseAIAgent<AttractiveShortDirectorInput, AttractiveShortDirectorOutput>(client, logger)
    {
        public override string Name => nameof(AttractiveShortDirector);

        protected override Type InstructionsResourceBaseType => this.GetType();

        protected override ReasoningEffort Reasoning => ReasoningEffort.Medium;
    }
}
