using Microsoft.Agents.AI.Workflows;

namespace Niobium.AI
{
    public class UserInputAdaptor<T>() : Executor<T, T>(nameof(UserInputAdaptor<T>))
    {
        public override async ValueTask<T> HandleAsync(T message, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            await context.QueueStateUpdateAsync(States.UserInput, message, scopeName: States.SharedScope, cancellationToken: cancellationToken);
            return message;
        }
    }
}
