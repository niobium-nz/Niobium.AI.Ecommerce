using Microsoft.Agents.AI.Workflows;

namespace Niobium.AI
{
    public abstract class Sora2VideoAgent<T>(IVideoClientFactory videoClientFactory) : IVideoAgent<T> where T : IVideoInstruction
    {
        public abstract string Id { get; }

        protected virtual string Model => Models.SORA_2;

        protected IVideoClient VideoClient => videoClientFactory.CreateVideoClient(this.Model);

        protected virtual Task OnGettingResponseAsync(string conversationID, T input, CancellationToken cancellationToken)
            => Task.CompletedTask;

        public virtual async Task<Stream> GetResponseAsync(string conversationID, T input, CancellationToken cancellationToken)
        {
            // align size to 720p if necessary due to Sora2 limitations
            if (input.VideoWidth > 720)
            {
                var scale = input.VideoWidth / 720.0d;
                input.VideoWidth = 720;
                input.VideoHeight = (int)(input.VideoHeight / scale);
            }

            await this.OnGettingResponseAsync(conversationID, input, cancellationToken);
            var videoStream = await this.VideoClient.RunAsync(
                 conversationID,
                 input.VideoPrompt,
                 input.VideoWidth,
                 input.VideoHeight,
                 input.VideoDurationInSeconds,
                 cancellationToken);
            return await this.OnResponseGotAsync(conversationID, input, videoStream, cancellationToken);
        }

        protected virtual Task<Stream> OnResponseGotAsync(string conversationID, T input, Stream videoStream, CancellationToken cancellationToken)
            => Task.FromResult(videoStream);

        public ExecutorBinding GetBinding(string ? outputStateKey = null, string? stateScope = null, bool yieldWorkflowOutput = false)
            => new AgentExecutorAdaptor<T, Stream>(this, yieldWorkflowOutput, outputStateKey, stateScope);
    }
}
