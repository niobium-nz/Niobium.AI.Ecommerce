using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Responses;

namespace Niobium.AI
{
    public abstract class GenericResponseAIAgent<TInput, TOutput>(OpenAIClient client, ILogger logger) : GenericResponseAIAgent(client, logger), IResponseAgent<TInput, TOutput>
        where TOutput : class
    {
        public virtual async Task<TOutput> RunAsync(string conversationID, TInput input, CancellationToken cancellationToken)
        {
            var request = input is string str ? str : JsonSerializer.Serialize(input, SerializationOptions.SnakeCase);
            var agent = await this.GetOrCreateAgentAsync(conversationID, cancellationToken);
            AgentResponse<TOutput> response = await agent.RunAsync<TOutput>(request, cancellationToken: cancellationToken);
            this.LogUsage(conversationID, response.Usage);
            return response.Result;
        }
    }

    public abstract class GenericResponseAIAgent(OpenAIClient client, ILogger logger) : IAgent
    {
        private AIAgent? _agent;

        protected virtual string Model => Models.GPT_5_3;

        protected virtual ReasoningEffort Reasoning => ReasoningEffort.None;

        public abstract string Name { get; }

        protected virtual Task<IEnumerable<AITool>> GetToolsAsync(CancellationToken cancellationToken) => Task.FromResult(Enumerable.Empty<AITool>());

        protected virtual Type InstructionsResourceBaseType => this.GetType();

        protected virtual async Task<string> GetInstructionsAsync(CancellationToken cancellationToken)
        {
            var resource = $"{InstructionsResourceBaseType.Namespace}.{this.Name}.md";
            using var stream = InstructionsResourceBaseType.Assembly.GetManifestResourceStream(resource)!;
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
                logger.LogInformation("Agent {AgentName} token usage for conversation {ConversationId}: Input={InputToken}, Output={OutputToken}, Reasoning={ReasoningToken}, Total={TotalToken}",
                    this.Name, conversationID, inputToken, outputToken, reasoningToken, totalToken);
            }
        }

        protected virtual async Task<AIAgent> GetOrCreateAgentAsync(string conversationID, CancellationToken cancellationToken)
        {
            if (this._agent == null)
            {
                var instructions = await this.GetInstructionsAsync(cancellationToken);
                var tools = await this.GetToolsAsync(cancellationToken);
                var chatOptions = new ChatOptions
                {
                    Instructions = instructions,
                    Reasoning = new ReasoningOptions { Effort = this.Reasoning },
                    Tools = [.. tools],
                };

                ResponsesClient responsesClient = client.GetResponsesClient(this.Model);
                this._agent = responsesClient.AsAIAgent(new ChatClientAgentOptions
                {
                    Name = this.Name,
                    ChatOptions = chatOptions,
                    AIContextProviders = [],
                });
            }

            return this._agent;
        }

        protected virtual async Task<string> RunAsync(string conversationID, string input, CancellationToken cancellationToken)
        {
            var agent = await this.GetOrCreateAgentAsync(conversationID, cancellationToken);
            AgentResponse response = await agent.RunAsync(input, cancellationToken: cancellationToken);
            this.LogUsage(conversationID, response.Usage);
            return response.Text;
        }
    }
}
