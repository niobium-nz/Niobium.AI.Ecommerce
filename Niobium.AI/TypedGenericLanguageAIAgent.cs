using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Logging;

namespace Niobium.AI
{
    public abstract class TypedGenericLanguageAIAgent<TInput, TOutput>(IChatClientFactory clientFactory, ILogger logger) : GenericLanguageAIAgent(clientFactory, logger), IResponseAgent<TInput, TOutput>
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
