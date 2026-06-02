using Microsoft.Agents.AI.DurableTask;
using Microsoft.DurableTask;

namespace Niobium.AI
{
    public static class TaskOrchestrationContextExtensions
    {
        public static IResponseGenerator<TInput, TOutput> GetAgent<TAgent, TInput, TOutput>(this TaskOrchestrationContext context)
            where TAgent : class, IResponseGenerator<TInput, TOutput>
            where TInput : notnull
            where TOutput : class
        {
            Type agentType = typeof(TAgent);
            string agentName = agentType.Name;
            if (typeof(GenericResponseAgent).IsAssignableFrom(agentType))
            {
                DurableAIAgent agent = context.GetAgent(agentName);
                return new DurableAgentWrapper<TInput, TOutput>(agent);
            }
            else
            {
                return new DurableActivityAdaptor<TInput, TOutput>(context, agentName);
            }
        }
    }
}
