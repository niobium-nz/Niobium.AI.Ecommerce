using Azure;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.Logging;

namespace Niobium.AI.Shorts.Executors
{
    internal class FileUploader(IFileStorage fileStorage, ILogger<FileUploader> logger) : Executor<Stream, Uri>(nameof(FileUploader))
    {
        private static readonly Random random = new();

        public override async ValueTask<Uri> HandleAsync(Stream message, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            string videoName = $"{DateTime.Now:yyyyMMdd}-{random.Next(10, 99)}.mp4";
            try
            {
                logger.LogInformation($"Staging video {videoName} on Azure Blob...");
                Uri result = await fileStorage.UploadAsync(videoName, message, cancellationToken);
                logger.LogInformation($"Video {videoName} staged on Azure Blob as {result}");
                message.Dispose();
                return result;
            }
            catch (RequestFailedException e) when (e.Status == 409 && e.ErrorCode == "BlobAlreadyExists")
            {
                // If the blob already exists, it means we got lucky on the same random number. We can just need to call self again so another random name will be generated.
                logger.LogWarning("Blob with name {BlobName} already exists. Retrying with a new name...", videoName);
                return await this.HandleAsync(message, context, cancellationToken);
            }
        }
    }
}
