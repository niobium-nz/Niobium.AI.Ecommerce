namespace Niobium.AI
{
    public abstract class GenericVideoProducer<T>(IVideoClientFactory clientFactory) : IVideoProducer<T> where T : IVideoInstruction
    {
        public abstract string Id { get; }

        protected virtual string Model => Models.SORA_LATEST;

        protected virtual Task OnGettingResponseAsync(string conversationID, T input, CancellationToken cancellationToken)
            => Task.CompletedTask;

        public virtual async Task<Stream> GetResponseAsync(string conversationID, T input, CancellationToken cancellationToken)
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
            Stream videoStream = await client.RunAsync(
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
    }
}
