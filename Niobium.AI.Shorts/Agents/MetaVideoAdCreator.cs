using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Niobium.AI.Shorts.Contracts;

namespace Niobium.AI.Shorts.Agents
{
    internal class MetaVideoAdCreator(
        IChatClientFactory clientFactory,
        McpTools tools,
        ILogger<MetaVideoAdCreator> logger)
        : TypedGenericLanguageAIAgent<MetaVideoAdCreatorInput, string>(clientFactory, logger)
    {
        public override string Name => nameof(MetaVideoAdCreator);

        protected override Type InstructionsResourceBaseType => this.GetType();

        protected override ReasoningEffort Reasoning => ReasoningEffort.High;

        protected override Task<IEnumerable<AITool>> GetToolsAsync(CancellationToken cancellationToken) => tools.GetPlaywrightToolsAsync(cancellationToken);

        protected override DirectoryInfo? SkillsFolder => new(Path.Combine(AppContext.BaseDirectory, "skills"));

        protected override async Task OnResponseGotAsync(string conversationID, MetaVideoAdCreatorInput input, string? output, CancellationToken cancellationToken)
        {
            await tools.CleanupPlaywrightTabsAsync(cancellationToken);
            await base.OnResponseGotAsync(conversationID, input, output, cancellationToken);
        }
    }
}
