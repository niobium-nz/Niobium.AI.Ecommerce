using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Logging;

namespace Niobium.AI
{
    public abstract class TypedResponseAgent<TInput, TOutput>(IChatClientFactory clientFactory, ILogger logger)
        : GenericResponseAgent(clientFactory, logger), IResponseGenerator<TInput, TOutput>
        where TOutput : class
    {
        public virtual async Task<TOutput> RunAsync(TInput input, CancellationToken? cancellationToken = null)
        {
            cancellationToken ??= CancellationToken.None;
            AIAgent agent = this.GetOrCreateAgent();
            string request = input is string str ? str : JsonSerializer.Serialize(input, SerializationOptions.SnakeCase);
            AgentResponse response;
            TOutput? output;
            if (typeof(TOutput) == typeof(string))
            {
                response = await agent.RunAsync(request, cancellationToken: cancellationToken.Value);
                output = response.Text as TOutput;
            }
            else
            {
                AgentResponse<TOutput> resp = await agent.RunAsync<TOutput>(request, cancellationToken: cancellationToken.Value);
                output = resp.Result;
            }

            return output!;
        }
    }
}
