namespace Niobium.AI
{
    public abstract class GenericVideoProducer<TInput, TOutput>(IVideoClientFactory clientFactory) : IVideoProducer<TInput, TOutput> where TInput : IVideoInstruction
    {
        public abstract string Id { get; }

        protected virtual string? ModelProvider => null;

        protected virtual string Model => Models.SORA_LATEST;

        protected virtual Task OnGettingResponseAsync(TInput input, CancellationToken cancellationToken)
            => Task.CompletedTask;

        public virtual async Task<TOutput> RunAsync(TInput input, CancellationToken? cancellationToken = null)
        {
            cancellationToken ??= CancellationToken.None;
            // align size to 720p if necessary due to Sora2 limitations
            if (input.VideoWidth > 720)
            {
                double scale = input.VideoWidth / 720.0d;
                input.VideoWidth = 720;
                input.VideoHeight = (int)(input.VideoHeight / scale);
            }

            await this.OnGettingResponseAsync(input, cancellationToken.Value);
            IVideoClient client = clientFactory.CreateClient(this.Model, this.ModelProvider);
            BinaryData video = await client.RunAsync(
                 input.VideoPrompt,
                 input.VideoWidth,
                 input.VideoHeight,
                 input.VideoDurationInSeconds,
                 cancellationToken.Value);
            return await this.OnResponseGotAsync(input, video, cancellationToken.Value);
        }

        protected abstract Task<TOutput> OnResponseGotAsync(TInput input, BinaryData video, CancellationToken cancellationToken);
    }
}
