using Azure;
using Microsoft.Extensions.Logging;
using Niobium.AI.Shorts.Contracts;
using Niobium.AI.Shorts.Skills;

namespace Niobium.AI.Shorts.Executors
{
    internal class AttractiveShortProducer(
        IVideoClientFactory videoClientFactory,
        ILogger<AttractiveShortProducer> logger)
        : GenericVideoProducer<AttractiveShortScreenwriterOutput, Uri>(videoClientFactory)
    {
        private static readonly Random random = new();

        public override string Id => nameof(AttractiveShortProducer);

        protected override async Task<Uri> OnResponseGotAsync(AttractiveShortScreenwriterOutput input, BinaryData video, CancellationToken cancellationToken)
        {
            using Stream videoStream = video.ToStream();
            using Stream videoStreamWithSubtitle = await BurnSubtitleToVideo.BurnInSubtitlesAsync(videoStream, input, input.SubtitlePlan, cancellationToken);
            return await this.UploadAsync(videoStreamWithSubtitle, cancellationToken);
        }

        public async ValueTask<Uri> UploadAsync(Stream message, CancellationToken cancellationToken = default)
        {
            string videoName = $"{DateTime.Now:yyyyMMdd}-{random.Next(10, 99)}.mp4";
            try
            {
                logger.LogInformation($"Staging video {videoName} to storage...");
                if (message.CanSeek)
                {
                    message.Position = 0;
                }

                string path = $"/artifacts/shorts/{videoName}";
                using FileStream fileStream = new(path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
                await message.CopyToAsync(fileStream);
                logger.LogInformation($"Video {videoName} staged to storage as {path}");
                return new Uri(path);
            }
            catch (RequestFailedException e) when (e.Status == 409 && e.ErrorCode == "BlobAlreadyExists")
            {
                // If the blob already exists, it means we got lucky on the same random number. We can just need to call self again so another random name will be generated.
                logger.LogWarning("Blob with name {BlobName} already exists. Retrying with a new name...", videoName);
                return await this.UploadAsync(message, cancellationToken);
            }
        }
    }
}
