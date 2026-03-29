using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Logging;

namespace Niobium.AI
{
    public abstract class TypedResponseAgent<TInput, TOutput>(IChatClientFactory clientFactory, ILogger logger) : GenericResponseAgent(clientFactory, logger), IResponseGenerator<TInput, TOutput>
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
                string request = input is string str ? str : JsonSerializer.Serialize(input, SerializationOptions.SnakeCase);
                AIAgent agent = await this.GetOrCreateAgentAsync(conversationID, cancellationToken);

                //                System.ClientModel.ClientResultException: 'HTTP 429 (new_api_error: )
                //当前分组上游负载已饱和，请稍后再试(request id: 20260316171712186993607SDHgN0RI)'
                //Status = 429
                //Source = OpenAI

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
