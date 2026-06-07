using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Niobium.AI.Ecommerce.Contracts;

namespace Niobium.AI.Ecommerce.Agents
{
    internal class KeywordsExpander(IChatClientFactory clientFactory, Tools.ToolBox tools, ILogger<KeywordsExpander> logger)
        : TypedResponseAgent<KeywordsExpanderInput, KeywordsExpanderOutput>(clientFactory, logger)
    {
        public override string Id => nameof(KeywordsExpander);

        protected override ReasoningEffort Reasoning => ReasoningEffort.Medium;

        protected override IEnumerable<AITool> GetTools() => [tools.WebSearchTool];
    }
}
