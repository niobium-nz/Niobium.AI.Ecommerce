using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Responses;

namespace Niobium.Ads.Analyst
{
    internal abstract class GenericAIAgent<TInput, TOutput>(OpenAIClient client, ILogger logger) : GenericAIAgent(client, logger), IAgent<TInput, TOutput>
        where TOutput : class
    {
        protected override Type ResponseType => typeof(TOutput);

        protected override JsonSerializerOptions? SerializerOptions => SerializationOptions.SnakeCase;

        public virtual async Task<TOutput> RunAsync(string conversationID, TInput input, CancellationToken cancellationToken)
        {
            var request = input is string str ? str : JsonSerializer.Serialize(input, SerializationOptions.SnakeCase);
            var responseJSON = await base.RunAsync(conversationID, request, cancellationToken);
            if (typeof(TOutput) == typeof(string))
            {
                return responseJSON as TOutput ?? throw new AgentException($"Expected string output but got null. Response JSON: {responseJSON}");
            }

            TOutput? result = null;
            if (!String.IsNullOrWhiteSpace(responseJSON))
            {
                var normalizedJSON = responseJSON;
                if (normalizedJSON.StartsWith("```json", StringComparison.InvariantCultureIgnoreCase))
                {
                    normalizedJSON = normalizedJSON[7..];
                }

                if (normalizedJSON.EndsWith("```"))
                {
                    normalizedJSON = normalizedJSON[..^3];
                }

                result = JsonSerializer.Deserialize<TOutput>(normalizedJSON, SerializationOptions.SnakeCase);
            }

            return result ?? throw new AgentException($"Failed to deserialize agent response into {typeof(TOutput).Name}. Response JSON: {responseJSON}");
        }
    }

    internal abstract class GenericAIAgent(OpenAIClient client, ILogger logger) : IAgent
    {
        private AIAgent? _agent;

        protected virtual string Model => Models.GPT_5_2;

        protected virtual ReasoningEffort Reasoning => ReasoningEffort.None;

        protected virtual Type ResponseType => typeof(string);

        protected virtual JsonSerializerOptions? SerializerOptions => null;

        public abstract string Name { get; }

        protected virtual Task<IEnumerable<AITool>> GetToolsAsync(CancellationToken cancellationToken) => Task.FromResult(Enumerable.Empty<AITool>());

        protected virtual async Task<string> GetInstructionsAsync(CancellationToken cancellationToken)
        {
            var resourceName = $"{this.GetType().Namespace}.Agents.{this.Name}.md";
            using var stream = this.GetType().Assembly.GetManifestResourceStream(resourceName)!;
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync(cancellationToken);
        }

        protected virtual async Task<AIAgent> GetOrCreateAgentAsync(string conversationID, CancellationToken cancellationToken)
        {
            if (this._agent == null)
            {
                var instructions = await this.GetInstructionsAsync(cancellationToken);
                var tools = await GetToolsAsync(cancellationToken);
                var chatOptions = new ChatOptions
                {
                    Instructions = instructions,
                    Reasoning = new ReasoningOptions { Effort = this.Reasoning },
                    Tools = [.. tools],
                };
                if (this.ResponseType != typeof(string))
                {
                    chatOptions.ResponseFormat = ChatResponseFormat.ForJsonSchema(
                        this.ResponseType,
                        serializerOptions: this.SerializerOptions,
                        schemaName: this.ResponseType.Name);
                }

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

        public virtual async Task<string> RunAsync(string conversationID, string input, CancellationToken cancellationToken)
        {
            var agent = await this.GetOrCreateAgentAsync(conversationID, cancellationToken);
            AgentResponse response = await agent.RunAsync(input, cancellationToken: cancellationToken);
            if (response.Usage != null)
            {
                var inputToken = response.Usage.InputTokenCount;
                var outputToken = response.Usage.OutputTokenCount;
                var reasoningToken = response.Usage.ReasoningTokenCount;
                var totalToken = response.Usage.TotalTokenCount;
                logger.LogInformation("Agent {AgentName} token usage for conversation {ConversationId}: Input={InputToken}, Output={OutputToken}, Reasoning={ReasoningToken}, Total={TotalToken}",
                    this.Name, conversationID, inputToken, outputToken, reasoningToken, totalToken);
            }

            return response.Text;
        }
    }
}
