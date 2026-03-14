using Azure;
using Microsoft.Extensions.Logging;

namespace Niobium.AI
{
    public abstract class GenericVideoAIAgent<TInput, TOutput>(
        IFileStorage fileStorage,
        IVideoClientFactory videoClientFactory,
        IChatClientFactory chatClientFactory,
        ILogger logger)
        : TypedGenericLanguageAIAgent<TInput, TOutput>(chatClientFactory, logger), IVideoAgent<TInput, TOutput>
            where TOutput : class, IResponseWithVideo
    {
        private static readonly Random random = new();

        protected virtual string VideoModel => Models.SORA_2;

        protected IVideoClient VideoClient => videoClientFactory.CreateVideoClient(this.VideoModel);

        protected virtual Task OnGettingVideoAsync(string conversationID, TInput input, CancellationToken cancellationToken)
            => Task.CompletedTask;

        public virtual async Task<TOutput> GetVideoAsync(string conversationID, TInput input, CancellationToken cancellationToken)
        {
            var response = await this.GetResponseAsync(conversationID, input, cancellationToken);
            if (String.IsNullOrWhiteSpace(response.VideoPrompt))
            {
                throw new AgentException("Video prompt not found from the LLM response.");
            }

            // align size to 720p if necessary
            if (response.VideoWidth > 720)
            {
                var scale = response.VideoWidth / 720.0d;
                response.VideoWidth = 720;
                response.VideoHeight = (int)(response.VideoHeight / scale);
            }

            using var videoStream = await this.VideoClient.RunAsync(
                 conversationID,
                 response.VideoPrompt,
                 response.VideoWidth,
                 response.VideoHeight,
                 response.VideoDurationInSeconds,
                 cancellationToken);
            await this.OnVideoGotAsync(conversationID, input, response, videoStream, cancellationToken);
            return response;
        }

        protected virtual async Task OnVideoGotAsync(string conversationID, TInput input, TOutput output, Stream videoStream, CancellationToken cancellationToken)
        {
            var videoName = $"{DateTime.Now:yyyyMMdd}-{random.Next(10, 99)}.mp4";
            try
            {
                this.Logger.LogInformation($"Staging video {videoName} on Azure Blob...");
                var fileUrl = await fileStorage.UploadAsync(videoName, videoStream, cancellationToken);
                output.VideoUrl = fileUrl.ToString();
                this.Logger.LogInformation($"Video {videoName} staged on Azure Blob as {fileUrl}");
            }
            catch (RequestFailedException e) when (e.Status == 409)
            {
                // If the blob already exists, it means we got lucky on the same random number. We can just need to call self again so another random name will be generated.
                if (e.ErrorCode == "BlobAlreadyExists")
                {
                    await this.OnVideoGotAsync(conversationID, input, output, videoStream, cancellationToken);
                }
            }

        }
    }
}
