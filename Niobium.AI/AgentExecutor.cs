using Microsoft.Agents.AI.Workflows;

namespace Niobium.AI
{
    public class AgentExecutor<TInput, TOutput>(IResponseGenerator<TInput, TOutput> agent, bool yieldWorkflowOutput, string? outputStateKey = null, string? stateScope = null)
        : Executor<TInput, TOutput>(agent.Id)
    {
        public override async ValueTask<TOutput> HandleAsync(TInput message, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            Guid conversationId = Guid.NewGuid(); //TODO - how to get session ID from context as the conversation ID?
            TOutput? output = await agent.GetResponseAsync(conversationId.ToString(), message, cancellationToken);
            if (!String.IsNullOrEmpty(outputStateKey))
            {
                if (String.IsNullOrEmpty(stateScope))
                {
                    throw new InvalidOperationException("State scope must be provided when output state key is specified.");
                }

                await context.QueueStateUpdateAsync(outputStateKey, output, scopeName: stateScope, cancellationToken: cancellationToken);
            }

            if (yieldWorkflowOutput && output != null)
            {
                await context.YieldOutputAsync(output, cancellationToken);
            }

            return output;
        }
    }
}
