using Microsoft.Agents.AI.Workflows;

namespace Niobium.AI
{
    public static class IWorkflowContextExtensions
    {
        public static async Task<T> GetUserInput<T>(this IWorkflowContext context, CancellationToken cancellationToken = default)
            => await context.ReadStateAsync<T>(States.UserInput, States.SharedScope, cancellationToken)
                ?? throw new ExecutorException("Cannot retrieve user input from workflow context.");

        public static async Task SetUserInput<T>(this IWorkflowContext context, T input, CancellationToken cancellationToken = default)
            => await context.QueueStateUpdateAsync(States.UserInput, input, scopeName: States.SharedScope, cancellationToken: cancellationToken);
    }
}
