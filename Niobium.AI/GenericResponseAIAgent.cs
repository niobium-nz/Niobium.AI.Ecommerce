using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using OpenAI;

namespace Niobium.AI
{
    public abstract class GenericResponseAIAgent(OpenAIClient client, ILogger logger) : IAgent
    {
        private AIAgent? _agent;

        protected ILogger Logger { get; } = logger ?? throw new ArgumentNullException(nameof(logger));

        protected virtual string Model => Models.GPT_5_3;

        protected virtual ReasoningEffort Reasoning => ReasoningEffort.None;

        public abstract string Name { get; }

        protected virtual Task<IEnumerable<AITool>> GetToolsAsync(CancellationToken cancellationToken) => Task.FromResult(Enumerable.Empty<AITool>());

        protected virtual Type InstructionsResourceBaseType => this.GetType();

        protected virtual async Task<string> GetInstructionsAsync(CancellationToken cancellationToken)
        {
            var resource = $"{this.InstructionsResourceBaseType.Namespace}.{this.Name}.md";
            using var stream = this.InstructionsResourceBaseType.Assembly.GetManifestResourceStream(resource)
                ?? throw new InvalidOperationException($"Instructions resource not found: {resource}");
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync(cancellationToken);
        }

        protected void LogUsage(string conversationID, UsageDetails? usage)
        {
            if (usage != null)
            {
                var inputToken = usage.InputTokenCount;
                var outputToken = usage.OutputTokenCount;
                var reasoningToken = usage.ReasoningTokenCount;
                var totalToken = usage.TotalTokenCount;
                this.Logger.LogInformation("Agent {AgentName} token usage for conversation {ConversationId}: Input={InputToken}, Output={OutputToken}, Reasoning={ReasoningToken}, Total={TotalToken}",
                    this.Name, conversationID, inputToken, outputToken, reasoningToken, totalToken);
            }
        }

        protected virtual async Task<AIAgent> GetOrCreateAgentAsync(string conversationID, CancellationToken cancellationToken)
        {
            if (this._agent == null)
            {
                var instructions = await this.GetInstructionsAsync(cancellationToken);
                var tools = (await this.GetToolsAsync(cancellationToken)) ?? [];
                var chatOptions = new ChatOptions
                {
                    Instructions = instructions,
                    Reasoning = new ReasoningOptions { Effort = this.Reasoning },
                    Tools = tools.ToList(),
                };

                this._agent = client.GetResponsesClient(this.Model)
                    .AsIChatClient()
                    .AsBuilder()
                    .UseOpenTelemetry(
                        sourceName: "Niobium.AI",
                        configure: cfg => cfg.EnableSensitiveData = true)
                    .BuildAIAgent(new ChatClientAgentOptions
                    {
                        Name = this.Name,
                        ChatOptions = chatOptions,
                    });
            }

            return this._agent!;
        }

        protected virtual async Task<string> RunAsync(string conversationID, string input, CancellationToken cancellationToken)
        {
            var agent = await this.GetOrCreateAgentAsync(conversationID, cancellationToken);
            AgentResponse response = await agent.RunAsync(input, cancellationToken: cancellationToken);
            this.LogUsage(conversationID, response.Usage);
            return response.Text;
        }
    }

    public abstract class GenericResponseAIAgent<TInput, TOutput>(OpenAIClient client, ILogger logger) : GenericResponseAIAgent(client, logger), IResponseAgent<TInput, TOutput>
        where TOutput : class
    {
        protected virtual Task OnRunningAsync(string conversationID, TInput input, CancellationToken cancellationToken) => Task.CompletedTask;

        protected virtual Task OnRanAsync(string conversationID, TInput input, TOutput? output, CancellationToken cancellationToken) => Task.CompletedTask;

        public virtual async Task<TOutput> RunAsync(string conversationID, TInput input, CancellationToken cancellationToken)
        {
            TOutput? output = null;
            try
            {
                await this.OnRunningAsync(conversationID, input, cancellationToken);
                var request = input is string str ? str : JsonSerializer.Serialize(input, SerializationOptions.SnakeCase);
                var agent = await this.GetOrCreateAgentAsync(conversationID, cancellationToken);
                AgentResponse<TOutput> response = await agent.RunAsync<TOutput>(request, cancellationToken: cancellationToken);
                this.LogUsage(conversationID, response.Usage);
                output = response.Result;
                return output!;
            }
            finally
            {
                await this.OnRanAsync(conversationID, input, output, cancellationToken);
            }
        }
    }
}
