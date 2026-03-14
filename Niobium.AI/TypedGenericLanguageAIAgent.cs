using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Logging;

namespace Niobium.AI
{
    public abstract class TypedGenericLanguageAIAgent<TInput, TOutput>(IChatClientFactory clientFactory, ILogger logger) : GenericLanguageAIAgent(clientFactory, logger), IResponseAgent<TInput, TOutput>
        where TOutput : class
    {
        protected virtual Task OnGettingResponseAsync(string conversationID, TInput input, CancellationToken cancellationToken) => Task.CompletedTask;

        protected virtual Task OnResponseGotAsync(string conversationID, TInput input, TOutput? output, CancellationToken cancellationToken) => Task.CompletedTask;

        public virtual async Task<TOutput> GetResponseAsync(string conversationID, TInput input, CancellationToken cancellationToken)
        {
            TOutput? output = null;
            try
            {
                await this.OnGettingResponseAsync(conversationID, input, cancellationToken);
                var request = input is string str ? str : JsonSerializer.Serialize(input, SerializationOptions.SnakeCase);
                var agent = await this.GetOrCreateAgentAsync(conversationID, cancellationToken);

                AgentResponse response;
                if (typeof(TOutput) == typeof(string))
                {
                    response = await agent.RunAsync(request, cancellationToken: cancellationToken);
                    output = response.Text as TOutput;
                }
                else
                {
                    AgentResponse<TOutput> resp = await agent.RunAsync<TOutput>(request, cancellationToken: cancellationToken);
                    response = resp;
                    output = resp.Result;
                }
                
                this.LogUsage(conversationID, response.Usage);                
                return output!;
            }
            finally
            {
                await this.OnResponseGotAsync(conversationID, input, output, cancellationToken);
            }
        }
    }
}
