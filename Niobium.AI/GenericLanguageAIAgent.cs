using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Niobium.AI
{
    public abstract class GenericLanguageAIAgent(IChatClientFactory clientFactory, ILogger logger) : IAgent
    {
        private AIAgent? _agent;

        protected ILogger Logger { get; } = logger ?? throw new ArgumentNullException(nameof(logger));

        protected virtual bool KeepContext => true;

        protected virtual int MaxMessagesBeforeContextCompaction => -1;

        protected virtual int TrailingMessagesToKeepUnderContextCompaction => -1;

        protected virtual string Model => Models.GPT_LATEST;

        protected virtual ReasoningEffort Reasoning => ReasoningEffort.None;

        protected virtual DirectoryInfo? SkillsFolder => null;

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
                    Reasoning = new ReasoningOptions
                    {
                        Effort = this.Reasoning,
                    },
                    Tools = [.. tools],
                };

                IChatClient client = clientFactory.CreateChatClient(this.Model);
                if (!KeepContext)
                {
                    client = new CompactingChatClient(client, this.MaxMessagesBeforeContextCompaction, this.TrailingMessagesToKeepUnderContextCompaction);
                }

                this._agent = client.AsBuilder()
                    .UseOpenTelemetry(
                        sourceName: "Niobium.AI",
                        configure: cfg => cfg.EnableSensitiveData = true)
                    .BuildAIAgent(new ChatClientAgentOptions
                    {
                        Name = this.Name,
                        ChatOptions = chatOptions,
                        AIContextProviders = this.SkillsFolder != null
                            ? [new FileAgentSkillsProvider(skillPath: this.SkillsFolder.FullName)]
                            : null,
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
}
