using Microsoft.Agents.AI.Workflows;

namespace Niobium.AI
{
    public class UserInputAdaptor<T>() : Executor<T, T>($"{nameof(UserInputAdaptor<>)}_{typeof(T).Name}")
    {
        public override async ValueTask<T> HandleAsync(T message, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            await context.SetUserInput(message, cancellationToken);
            return message;
        }
    }
}
