using Microsoft.Extensions.AI;

namespace Niobium.AI
{
    public class ToolBox
    {
        public AITool WebSearchTool => new HostedWebSearchTool();

        public AITool CodeInterpreterTool => new HostedCodeInterpreterTool();
    }
}
