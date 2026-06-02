using System.Text.Json;
using Microsoft.Agents.AI.Workflows;
using Niobium.AI.Shorts.Contracts;
using Niobium.AI.Shorts.Executors;

namespace Niobium.AI.Shorts
{
    internal class AttractiveShortWorkflow(
        AttractiveShortScreenwriterAdaptor attractiveShortScreenwriterAdaptor,
        AttractiveShortScreenwriter attractiveShortScreenwriter,
        AttractiveShortProducer attractiveShortProducer,
        MetaVideoAdCreator metaVideoAdCreator,
        MetaVideoAdCreatorAdaptor metaVideoAdCreatorAdaptor)
        : IWorkflow<AttractiveShortWorkflowInput, AttractiveShortWorkflowOutput>
    {
        private Workflow? workflow;

        protected Workflow Workflow => this.GetOrCreateWorkflow();

        public string Id => nameof(AttractiveShortWorkflow);

        public string MermaidDiagram => this.Workflow.ToMermaidString();

        public async Task<AttractiveShortWorkflowOutput> RunAsync(AttractiveShortWorkflowInput input, CancellationToken? cancellationToken = null)
        {
            cancellationToken ??= CancellationToken.None;
            await using Run run = await InProcessExecution.RunAsync(this.Workflow, input, cancellationToken: cancellationToken.Value);
            foreach (WorkflowEvent evt in run.NewEvents)
            {
                if (evt is WorkflowOutputEvent outputEvt)
                {
                    return outputEvt.Data == null
                        ? throw new InvalidOperationException($"Workflow output event data is null.")
                        : outputEvt.Data is MetaVideoAdCreatorOutput output
                        ? new AttractiveShortWorkflowOutput
                        {
                            Status = output.Status,
                            AdName = output.AdName,
                            ScreenshotFullFilePath = output.ScreenshotFullFilePath
                        }
                        : throw new InvalidCastException($"Expected workflow output data of type {nameof(MetaVideoAdCreatorOutput)}, but got {outputEvt.Data.GetType().FullName}.");
                }
            }

            throw new InvalidOperationException($"Workflow {nameof(AttractiveShortWorkflow)} completed without producing an output.");
        }

        public async Task<string> RunAsync(string input, CancellationToken? cancellationToken = null)
            => JsonSerializer.Serialize(await this.RunAsync(JsonSerializer.Deserialize<AttractiveShortWorkflowInput>(input)!, cancellationToken));

        private Workflow GetOrCreateWorkflow()
        {
            if (this.workflow == null)
            {
                //ExecutorBinding attractiveShortScreenwriterBinding = attractiveShortScreenwriter.GetBinding(States.VideoInstructions, States.SharedScope);
                //ExecutorBinding attractiveShortProducerBinding = attractiveShortProducer.GetBinding();
                //ExecutorBinding metaVideoAdCreatorBinding = metaVideoAdCreator.GetBinding(yieldWorkflowOutput: true);

                //WorkflowBuilder builder = new WorkflowBuilder(attractiveShortScreenwriterAdaptor)
                //    .AddEdge(attractiveShortScreenwriterAdaptor, attractiveShortScreenwriterBinding)
                //    .AddEdge(attractiveShortScreenwriterBinding, attractiveShortProducerBinding)
                //    .AddEdge(attractiveShortProducerBinding, metaVideoAdCreatorAdaptor)
                //    .AddEdge(metaVideoAdCreatorAdaptor, metaVideoAdCreatorBinding)
                //    .WithOutputFrom(metaVideoAdCreatorBinding);
                this.workflow = new WorkflowBuilder(null!).Build();
            }

            return this.workflow;
        }
    }
}
