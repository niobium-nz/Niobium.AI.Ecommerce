using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Niobium.AI.OpenAI
{
    internal class SoraVideoClient(HttpClient client, ILogger<SoraVideoClient> logger) : IVideoClient
    {
        public async Task<Stream> RunAsync(
            string conversationID,
            string prompt,
            int width,
            int height,
            int durationInSeconds,
            CancellationToken cancellationToken)
        {
            if (durationInSeconds is not 4 and not 8 and not 12)
            {
                throw new ArgumentOutOfRangeException(nameof(durationInSeconds));
            }

            var requestBody = new
            {
                prompt,
                size = $"{width}x{height}",
                seconds = durationInSeconds.ToString(),
                model = Models.SORA_2,
            };
            var json = JsonSerializer.Serialize(requestBody, SerializationOptions.SnakeCase);
            var requestContent = new StringContent(json, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, "videos")
            {
                Content = requestContent
            };

            var response = await client.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Sora API request failed with status code {StatusCode}: {ResponseBody}", response.StatusCode, await response.Content.ReadAsStringAsync(cancellationToken));
                throw new HttpRequestException($"Sora create API request failed with status code {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(responseContent);
            var videoId = document.RootElement.GetProperty("id").GetString();
            if (String.IsNullOrWhiteSpace(videoId))
            {
                var ex = new AgentException("Failed to get video ID from Sora response");
                logger.LogError(ex, "Failed to get video ID from Sora response: {ResponseContent}", responseContent);
                throw ex;
            }

            logger.LogInformation("Sora video generation started with ID: {VideoId}", videoId);

            SoraJobQuery? result = null;
            while (result == null || result.Status != "completed")
            {
                HttpRequestMessage queryRequest = new(HttpMethod.Get, $"videos/{videoId}");
                var queryResponse = await client.SendAsync(queryRequest, cancellationToken);
                if (!queryResponse.IsSuccessStatusCode)
                {
                    logger.LogError("Sora API request failed with status code {StatusCode}: {ResponseBody}", queryResponse.StatusCode, await queryResponse.Content.ReadAsStringAsync(cancellationToken));
                    throw new HttpRequestException($"Sora query API request failed with status code {queryResponse.StatusCode}");
                }

                var queryResponseContent = await queryResponse.Content.ReadAsStringAsync(cancellationToken);
                result = JsonSerializer.Deserialize<SoraJobQuery>(queryResponseContent, SerializationOptions.SnakeCase);

                if (result?.Status == "failed")
                {
                    var ex = new AgentException("Sora video generation failed");
                    logger.LogError(ex, "Sora video generation failed with code {Code}: {ErrorMessage}", result?.Error?.Code, result?.Error?.Message);
                    throw ex;
                }

                if (result?.Status == "cancelled" || cancellationToken.IsCancellationRequested)
                {
                    var ex = new OperationCanceledException("Sora video generation was cancelled", cancellationToken);
                    logger.LogInformation(ex, "Sora video generation was cancelled");
                    throw ex;
                }

                logger.LogInformation("Sora video generation status: {Status}, current progress: {Progress}. Checking again in 20 seconds...", result?.Status, result?.Progress);
                await Task.Delay(TimeSpan.FromSeconds(20), cancellationToken);
            }

            logger.LogInformation("Sora video generation completed for ID: {VideoId}. Starting download...", videoId);
            HttpRequestMessage downloadRequest = new(HttpMethod.Get, $"videos/{videoId}/content");
            var downloadResponse = await client.SendAsync(downloadRequest, cancellationToken);
            if (!downloadResponse.IsSuccessStatusCode)
            {
                logger.LogError("Sora API request failed with status code {StatusCode}: {ResponseBody}", downloadResponse.StatusCode, await downloadResponse.Content.ReadAsStringAsync(cancellationToken));
                throw new HttpRequestException($"Sora download API request failed with status code {downloadResponse.StatusCode}");
            }

            using var video = await downloadResponse.Content.ReadAsStreamAsync(cancellationToken);
            var memoryStream = new MemoryStream();
            if (video.CanSeek)
            {
                video.Seek(0, SeekOrigin.Begin);
            }
            await video.CopyToAsync(memoryStream, cancellationToken);
            logger.LogInformation("Sora video downloaded for ID {VideoId}", videoId);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
        }
    }
}
