namespace Niobium.AI
{
    public abstract class GenericVideoProducer<TInput, TOutput>(IVideoClientFactory clientFactory) : IVideoProducer<TInput, TOutput> where TInput : IVideoInstruction
    {
        public abstract string Id { get; }

        protected virtual string Model => Models.SORA_LATEST;

        protected virtual Task OnGettingResponseAsync(string conversationID, TInput input, CancellationToken cancellationToken)
            => Task.CompletedTask;

        public virtual async Task<TOutput> GetResponseAsync(string conversationID, TInput input, CancellationToken cancellationToken)
        {
            // align size to 720p if necessary due to Sora2 limitations
            if (input.VideoWidth > 720)
            {
                double scale = input.VideoWidth / 720.0d;
                input.VideoWidth = 720;
                input.VideoHeight = (int)(input.VideoHeight / scale);
            }

            await this.OnGettingResponseAsync(conversationID, input, cancellationToken);
            IVideoClient client = clientFactory.CreateClient(this.Model);
            BinaryData video = await client.RunAsync(
                 conversationID,
                 input.VideoPrompt,
                 input.VideoWidth,
                 input.VideoHeight,
                 input.VideoDurationInSeconds,
                 cancellationToken);
            return await this.OnResponseGotAsync(conversationID, input, video, cancellationToken);
        }

        protected abstract Task<TOutput> OnResponseGotAsync(string conversationID, TInput input, BinaryData video, CancellationToken cancellationToken);
    }
}
