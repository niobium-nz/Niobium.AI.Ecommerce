using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Niobium.AI.Shorts.Contracts;
using Niobium.AI.WebBrowser.Playwright;

namespace Niobium.AI.Shorts.Executors
{
    internal class MetaVideoAdCreator(
        IChatClientFactory clientFactory,
        IWebBrowser browser,
        ILogger<MetaVideoAdCreator> logger)
        : TypedResponseAgent<MetaVideoAdCreatorInput, MetaVideoAdCreatorOutput>(clientFactory, logger)
    {
        public override string Id => nameof(MetaVideoAdCreator);

        protected override Type InstructionsResourceBaseType => this.GetType();

        protected override ReasoningEffort Reasoning => ReasoningEffort.High;

        protected override IEnumerable<AITool> GetTools() => browser.AsAITools();

        protected override DirectoryInfo? SkillsFolder => new(Path.Combine(AppContext.BaseDirectory, "skills"));
    }
}
