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

        public virtual async Task<TOutput> GetVideoAsync(string conversationID, TInput input, CancellationToken cancellationToken)
        {
            var response = await this.GetResponseAsync(conversationID, input, cancellationToken);
            if (String.IsNullOrWhiteSpace(response.VideoPrompt))
            {
                throw new AgentException("Video prompt not found from the LLM response.");
            }

            var videoFile = await this.VideoClient.RunAsync(
                conversationID,
                response.VideoPrompt,
                response.VideoWidth,
                response.VideoHeight,
                response.VideoDurationInSeconds,
                cancellationToken);

            using var fs = videoFile.OpenRead();
            var videoName = $"{DateTime.Now:yyyyMMdd}-{random.Next(10, 99)}{videoFile.Extension}";
            Logger.LogInformation($"Staging video {videoName} on Azure Blob...");
            var fileUrl = await fileStorage.UploadAsync(videoName, fs, cancellationToken);
            response.VideoUrl = fileUrl;
            Logger.LogInformation($"Video {videoName} staged on Azure Blob as {fileUrl}");
            return response;
        }
    }
}
