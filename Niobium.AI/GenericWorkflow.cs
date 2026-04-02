using System.Text.Json;
using Microsoft.Agents.AI.Workflows;

namespace Niobium.AI
{
    public abstract class GenericWorkflow<TInput, TOutput>
        : IWorkflow<TInput, TOutput>
        where TInput : notnull
        where TOutput : class
    {
        protected Workflow? Workflow { get; private set; }

        public abstract string Id { get; }

        public string Render() => this.GetOrCreateWorkflow().ToMermaidString();

        public async Task<string> RunAsync(string conversationID, string input, CancellationToken cancellationToken)
            => JsonSerializer.Serialize(await this.RunAsync(conversationID, JsonSerializer.Deserialize<TInput>(input)!, cancellationToken));

        public async Task<TOutput?> RunAsync(string conversationID, TInput input, CancellationToken cancellationToken)
        {
            bool validationResult = this.ValidateInput(input);
            if (!validationResult)
            {
                return null;
            }

            Workflow workflow = this.GetOrCreateWorkflow();
            await using Run run = await InProcessExecution.RunAsync(workflow, input, sessionId: conversationID, cancellationToken: cancellationToken);
            foreach (WorkflowEvent evt in run.OutgoingEvents)
            {
                if (evt is WorkflowOutputEvent output)
                {
                    return output.Data == null ? null : output.Data as TOutput;
                }
            }

            return null;
        }

        private Workflow GetOrCreateWorkflow()
        {
            this.Workflow ??= this.BuildWorkflow();
            return this.Workflow;
        }

        protected abstract Workflow BuildWorkflow();

        protected virtual bool ValidateInput(TInput input) => true;
    }
}
