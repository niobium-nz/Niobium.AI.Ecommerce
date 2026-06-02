using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.DurableTask;
using Microsoft.Extensions.AI;

namespace Niobium.AI
{
    internal class DurableAgentWrapper<TInput, TOutput>(DurableAIAgent agent) : IResponseGenerator<TInput, TOutput>
        where TInput : notnull
        where TOutput : class
    {
        public string Id => agent.Id;

        public async Task<TOutput> RunAsync(TInput input, CancellationToken? cancellationToken = null)
        {
            string json = JsonSerializer.Serialize(input, options: SerializationOptions.SnakeCase);
            AgentResponse<TOutput> response = await agent.RunAsync<TOutput>(
                json, 
                options: new AgentRunOptions
                {
                    ResponseFormat = ChatResponseFormat.ForJsonSchema<TOutput>(serializerOptions: SerializationOptions.SnakeCase)
                },
                serializerOptions: SerializationOptions.SnakeCase,
                cancellationToken: cancellationToken ?? CancellationToken.None);
            return response.Result;
        }
    }
}
