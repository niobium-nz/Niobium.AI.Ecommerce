using Microsoft.Agents.AI.Workflows;

namespace Niobium.AI
{
    public static class IResponseGeneratorExtensions
    {
        public static ExecutorBinding GetBinding<TInput, TOutput>(this IResponseGenerator<TInput, TOutput> generator, string? outputStateKey = null, string? stateScope = States.SharedScope, bool yieldWorkflowOutput = false)
            => new AgentExecutor<TInput, TOutput>(generator, yieldWorkflowOutput, outputStateKey, stateScope);
    }
}
