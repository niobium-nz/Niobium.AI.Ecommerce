using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Niobium.AI
{
    public abstract class GenericResponseAgent(IChatClientFactory clientFactory, ILogger logger) : IExecutor
    {
        private AIAgent? _agent;

        protected ILogger Logger { get; } = logger ?? throw new ArgumentNullException(nameof(logger));

        protected virtual string? ModelProvider => null;

        protected virtual string Model => Models.GPT_LATEST;

        protected virtual ReasoningEffort Reasoning => ReasoningEffort.None;

        protected virtual DirectoryInfo? SkillsFolder => null;

        public abstract string Id { get; }

        protected virtual IEnumerable<AITool> GetTools() => [];

        protected virtual Type InstructionsResourceBaseType => this.GetType();

        protected virtual Type? ResponseType { get; } = null;

        protected virtual string GetInstructions()
        {
            string resource = $"{this.InstructionsResourceBaseType.Namespace}.{this.Id}.md";
            using Stream stream = this.InstructionsResourceBaseType.Assembly.GetManifestResourceStream(resource)
                ?? throw new InvalidOperationException($"Instructions resource not found: {resource}");
            using StreamReader reader = new(stream);
            return reader.ReadToEnd();
        }

        protected virtual async Task<string> RunAsync(string input, CancellationToken cancellationToken)
        {
            AIAgent agent = this.GetOrCreateAgent();
            AgentResponse response = await agent.RunAsync(input, cancellationToken: cancellationToken);
            return response.Text;
        }

        public virtual AIAgent GetOrCreateAgent()
        {
            if (this._agent == null)
            {
                string instructions = this.GetInstructions();
                IEnumerable<AITool> tools = this.GetTools() ?? [];
                ChatOptions chatOptions = new()
                {
                    Instructions = instructions,
                    Reasoning = new ReasoningOptions
                    {
                        Effort = this.Reasoning,
                    },
                    Tools = [.. tools],
                    AllowMultipleToolCalls = true,
                };

                if (this.ResponseType != null)
                {
                    chatOptions.ResponseFormat = ChatResponseFormat.ForJsonSchema(this.ResponseType, serializerOptions: SerializationOptions.SnakeCase);
                }

                ChatClientBuilder builder = clientFactory.CreateChatClient(this.Model, this.ModelProvider).AsBuilder();

                if (this.SkillsFolder != null)
                {
                    builder = builder.UseAIContextProviders(new AgentSkillsProvider(skillPath: this.SkillsFolder.FullName));
                }

                this._agent = builder.BuildAIAgent(new ChatClientAgentOptions
                {
                    Name = this.Id,
                    ChatOptions = chatOptions,
                });
            }

            return this._agent!;
        }
    }
}
