using System.Text.Json;
using Microsoft.Agents.AI.Workflows;
using Niobium.AI.Shorts.Contracts;
using Niobium.AI.Shorts.Executors;

namespace Niobium.AI.Shorts
{
    internal class AttractiveShortWorkflow(
        UserInputAdaptor<AttractiveShortWorkflowInput> workflowUserInputAdaptor,
        AttractiveShortScreenwriterAdaptor attractiveShortScreenwriterAdaptor,
        AttractiveShortScreenwriter attractiveShortScreenwriter,
        AttractiveShortProducer attractiveShortProducer,
        MetaVideoAdCreator metaVideoAdCreator,
        MetaVideoAdCreatorAdaptor metaVideoAdCreatorAdaptor)
        : IWorkflow<AttractiveShortWorkflowInput, AttractiveShortWorkflowOutput>
    {
        private Workflow? workflow;

        public string Id => nameof(AttractiveShortWorkflow);

        public string Render() => this.GetOrCreateWorkflow().ToMermaidString();

        public async Task<AttractiveShortWorkflowOutput?> RunAsync(string conversationID, AttractiveShortWorkflowInput input, CancellationToken cancellationToken)
        {
            Workflow workflow = this.GetOrCreateWorkflow();
            await using Run run = await InProcessExecution.RunAsync(workflow, input, sessionId: conversationID, cancellationToken: cancellationToken);
            foreach (WorkflowEvent evt in run.NewEvents)
            {
                if (evt is WorkflowOutputEvent outputEvt)
                {
                    return outputEvt.Data == null
                        ? throw new InvalidOperationException($"[{conversationID}] Workflow output event data is null.")
                        : outputEvt.Data is MetaVideoAdCreatorOutput output
                        ? new AttractiveShortWorkflowOutput
                        {
                            Status = output.Status,
                            AdName = output.AdName,
                            ScreenshotFullFilePath = output.ScreenshotFullFilePath
                        }
                        : throw new InvalidCastException($"[{conversationID}] Expected workflow output data of type {nameof(MetaVideoAdCreatorOutput)}, but got {outputEvt.Data.GetType().FullName}.");
                }
            }

            throw new InvalidOperationException($"[{conversationID}] Workflow {nameof(AttractiveShortWorkflow)} completed without producing an output.");
        }

        public async Task<string> RunAsync(string conversationID, string input, CancellationToken cancellationToken)
            => JsonSerializer.Serialize(await this.RunAsync(conversationID, JsonSerializer.Deserialize<AttractiveShortWorkflowInput>(input)!, cancellationToken));

        private Workflow GetOrCreateWorkflow()
        {
            if (this.workflow == null)
            {
                ExecutorBinding attractiveShortScreenwriterBinding = attractiveShortScreenwriter.GetBinding(States.VideoInstructions, States.SharedScope);
                ExecutorBinding attractiveShortProducerBinding = attractiveShortProducer.GetBinding();
                ExecutorBinding metaVideoAdCreatorBinding = metaVideoAdCreator.GetBinding(yieldWorkflowOutput: true);

                WorkflowBuilder builder = new WorkflowBuilder(workflowUserInputAdaptor)
                    .AddEdge(workflowUserInputAdaptor, attractiveShortScreenwriterAdaptor)
                    .AddEdge(attractiveShortScreenwriterAdaptor, attractiveShortScreenwriterBinding)
                    .AddEdge(attractiveShortScreenwriterBinding, attractiveShortProducerBinding)
                    .AddEdge(attractiveShortProducerBinding, metaVideoAdCreatorAdaptor)
                    .AddEdge(metaVideoAdCreatorAdaptor, metaVideoAdCreatorBinding)
                    .WithOutputFrom(metaVideoAdCreatorBinding);
                this.workflow = builder.Build();
            }

            return this.workflow;
        }
    }
}
