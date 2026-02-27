using System.ClientModel;
using System.Text.Json;
using Azure.AI.Projects;
using Azure.AI.Projects.OpenAI;
using Microsoft.Extensions.Logging;
using OpenAI.Responses;

namespace Niobium.Ads.Analyst
{
    internal abstract class HostedAIAgent<TInput, TOutput>(AIProjectClient client, ILogger logger) : HostedAIAgent(client, logger) , IAgent<TInput, TOutput>
        where TOutput : class
    {
        //protected override async Task<PromptAgentDefinition> BuildAgentDefinitionAsync(CancellationToken cancellationToken)
        //{
        //    var agentDefinition = await base.BuildAgentDefinitionAsync(cancellationToken);
        //    JsonNode schema = JsonSchemaExporter.GetJsonSchemaAsNode(SerializationOptions.SnakeCase, typeof(TOutput));
        //    string schemaInJSON = schema.ToString();
        //    agentDefinition.TextOptions = new ResponseTextOptions
        //    {
        //        TextFormat = ResponseTextFormat.CreateJsonSchemaFormat(typeof(TOutput).Name, BinaryData.FromString(schemaInJSON), jsonSchemaIsStrict: true)
        //    };

        //    return agentDefinition;
        //}

        public virtual async Task<TOutput> RunAsync(string conversationID, TInput input, CancellationToken cancellationToken)
        {
            var request = input is string str ? str : JsonSerializer.Serialize(input, SerializationOptions.SnakeCase);
            var responseJSON = await base.RunAsync(conversationID, request, cancellationToken);
            if (typeof(TOutput) == typeof(string))
            {
                return responseJSON as TOutput ?? throw new AgentException($"Expected string output but got null. Response JSON: {responseJSON}");
            }

            TOutput? result = string.IsNullOrWhiteSpace(responseJSON) ? null
                : JsonSerializer.Deserialize<TOutput>(responseJSON!, SerializationOptions.SnakeCase);
            return result ?? throw new AgentException($"Failed to deserialize agent response into {typeof(TOutput).Name}. Response JSON: {responseJSON}");
        }
    }

    internal abstract class HostedAIAgent(AIProjectClient client, ILogger logger) : IAgent
    {
        private ProjectResponsesClient? _agent;

        protected virtual string Model => Models.GPT_5_2;

        protected virtual ResponseReasoningEffortLevel? Reasoning => null;

        protected virtual IEnumerable<ResponseTool> Tools { get; } = [];

        public abstract string Name { get; }

        protected async virtual Task<string> GetInstructionsAsync(CancellationToken cancellationToken)
        {
            var resourceName = $"{this.GetType().Namespace}.Agents.{Name}.md";
            using var stream = this.GetType().Assembly.GetManifestResourceStream(resourceName)!;
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync(cancellationToken);
        }

        protected virtual async Task<PromptAgentDefinition> BuildAgentDefinitionAsync(CancellationToken cancellationToken)
        {
            PromptAgentDefinition agentDefinition = new(Model)
            {
                Instructions = await GetInstructionsAsync(cancellationToken),
            };

            foreach (var tool in Tools)
            {
                agentDefinition.Tools.Add(tool);
            }

            if (Reasoning != null)
            {
                agentDefinition.ReasoningOptions = new ResponseReasoningOptions
                {
                    ReasoningEffortLevel = Reasoning,
                };
            }

            return agentDefinition;
        }

        protected virtual async Task<AgentVersion> DeployAgentAsync(CancellationToken cancellationToken)
        {
            PromptAgentDefinition agentDefinition = await BuildAgentDefinitionAsync(cancellationToken);
            AgentVersion version = await client.Agents.CreateAgentVersionAsync(agentName: Name, options: new(agentDefinition), cancellationToken: cancellationToken);
            logger.LogInformation("Agent {AgentName} deployed with version {AgentVersion}", Name, version.Version);
            return version;
        }

        protected virtual async Task<ProjectResponsesClient> GetOrCreateAgentAsync(string conversationID, CancellationToken cancellationToken)
        {
            if (_agent == null)
            {
                AgentVersion version;

                try
                {
                    AgentRecord record = await client.Agents.GetAgentAsync(Name, cancellationToken);
                    version = record.Versions.Latest;
                }
                catch (ClientResultException e) when (e.Status == 404)
                {
                    logger.LogInformation("Agent {AgentName} not found. Deploying new agent.", Name);
                    version = await DeployAgentAsync(cancellationToken);
                }

                _agent = client.OpenAI.GetProjectResponsesClientForAgent(version);
            }

            return _agent;
        }

        public async Task DeployAsync(CancellationToken cancellationToken) => await DeployAgentAsync(cancellationToken);

        public virtual async Task<string> RunAsync(string conversationID, string input, CancellationToken cancellationToken)
        {
            var agent = await GetOrCreateAgentAsync(conversationID, cancellationToken);
            ResponseResult response = await agent.CreateResponseAsync(userInputText: input, cancellationToken: cancellationToken);
            var status = response.Status;
            var conversation = response.AgentConversationId;
            var usage = response.Usage;
            var reasoning = response.ReasoningOptions;
            var output = response.GetOutputText();
            var error = response.Error;
            var temperature = response.Temperature;
            var toolChoice = response.ToolChoice;
            logger.LogInformation("Agent {AgentName} responded with status {Status}. ConversationID={ConversationId}, Usage={Usage}, Reasoning={Reasoning}, Temperature={Temperature}, Tool Choice={ToolChoice}:\n{Output}",
                Name, status, conversation, usage?.TotalTokenCount, reasoning?.ReasoningEffortLevel, temperature, toolChoice?.FunctionName, output);
            return output ?? throw new AgentException($"Failed to get output from agent response: {error}");
        }
    }
}
