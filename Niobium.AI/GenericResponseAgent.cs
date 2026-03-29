using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Niobium.AI
{
    public abstract class GenericResponseAgent(IChatClientFactory clientFactory, ILogger logger) : IExecutor
    {
        private AIAgent? _agent;

        protected ILogger Logger { get; } = logger ?? throw new ArgumentNullException(nameof(logger));

        protected virtual string Model => Models.GPT_LATEST;

        protected virtual ReasoningEffort Reasoning => ReasoningEffort.None;

        protected virtual DirectoryInfo? SkillsFolder => null;

        public abstract string Id { get; }

        protected virtual Task<IEnumerable<AITool>> GetToolsAsync(CancellationToken cancellationToken) => Task.FromResult(Enumerable.Empty<AITool>());

        protected virtual Type InstructionsResourceBaseType => this.GetType();

        protected virtual async Task<string> GetInstructionsAsync(CancellationToken cancellationToken)
        {
            string resource = $"{this.InstructionsResourceBaseType.Namespace}.{this.Id}.md";
            using Stream stream = this.InstructionsResourceBaseType.Assembly.GetManifestResourceStream(resource)
                ?? throw new InvalidOperationException($"Instructions resource not found: {resource}");
            using StreamReader reader = new(stream);
            return await reader.ReadToEndAsync(cancellationToken);
        }

        protected void LogUsage(string conversationID, UsageDetails? usage)
        {
            if (usage != null)
            {
                long? inputToken = usage.InputTokenCount;
                long? outputToken = usage.OutputTokenCount;
                long? reasoningToken = usage.ReasoningTokenCount;
                long? totalToken = usage.TotalTokenCount;
                this.Logger.LogInformation("Agent {AgentName} token usage for conversation {ConversationId}: Input={InputToken}, Output={OutputToken}, Reasoning={ReasoningToken}, Total={TotalToken}",
                    this.Id, conversationID, inputToken, outputToken, reasoningToken, totalToken);
            }
        }

        protected virtual async Task<AIAgent> GetOrCreateAgentAsync(string conversationID, CancellationToken cancellationToken)
        {
            if (this._agent == null)
            {
                string instructions = await this.GetInstructionsAsync(cancellationToken);
                IEnumerable<AITool> tools = (await this.GetToolsAsync(cancellationToken)) ?? [];
                ChatOptions chatOptions = new()
                {
                    Instructions = instructions,
                    Reasoning = new ReasoningOptions
                    {
                        Effort = this.Reasoning,
                    },
                    Tools = [.. tools],
                };

                ChatClientBuilder builder = clientFactory.CreateChatClient(this.Model)
                    .AsBuilder()
                    .UseOpenTelemetry(
                        sourceName: "Niobium.AI",
                        configure: cfg => cfg.EnableSensitiveData = true);

                if (this.SkillsFolder != null)
                {
                    builder = builder.UseAIContextProviders(new FileAgentSkillsProvider(skillPath: this.SkillsFolder.FullName));
                }

                this._agent = builder.BuildAIAgent(new ChatClientAgentOptions
                {
                    Name = this.Id,
                    ChatOptions = chatOptions,
                });
            }

            return this._agent!;
        }

        protected virtual async Task<string> RunAsync(string conversationID, string input, CancellationToken cancellationToken)
        {
            try
            {
                AIAgent agent = await this.GetOrCreateAgentAsync(conversationID, cancellationToken);
                AgentResponse response = await agent.RunAsync(input, cancellationToken: cancellationToken);
                this.LogUsage(conversationID, response.Usage);
                return response.Text;
            }
            finally
            {
                await this.OnCleanupAsync(cancellationToken);
            }
        }

        protected virtual Task OnCleanupAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
}
