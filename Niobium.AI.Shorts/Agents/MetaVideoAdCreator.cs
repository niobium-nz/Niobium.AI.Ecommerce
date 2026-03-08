using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Niobium.AI.Shorts.Contracts;

namespace Niobium.AI.Shorts.Agents
{
    internal class MetaVideoAdCreator(IChatClientFactory clientFactory, ILogger<MetaVideoAdCreator> logger)
        : TypedGenericLanguageAIAgent<MetaVideoAdCreatorInput, MetaVideoAdCreatorOutput>(clientFactory, logger)
    {
        public override string Name => nameof(MetaVideoAdCreator);

        protected override string Model => "qwen3.5-plus";

        protected override Type InstructionsResourceBaseType => this.GetType();

        protected override ReasoningEffort Reasoning => ReasoningEffort.High;

        protected override DirectoryInfo? SkillsFolder => new(Path.Combine(AppContext.BaseDirectory, "skills"));
    }
}
