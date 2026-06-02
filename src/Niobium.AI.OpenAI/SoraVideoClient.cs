using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Niobium.AI.OpenAI
{
    internal class SoraVideoClient(HttpClient client, ILogger<SoraVideoClient> logger) : IVideoClient
    {
        public async Task<BinaryData> RunAsync(
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
            string json = JsonSerializer.Serialize(requestBody, SerializationOptions.SnakeCase);
            StringContent requestContent = new(json, Encoding.UTF8, "application/json");
            HttpRequestMessage request = new(HttpMethod.Post, "videos")
            {
                Content = requestContent
            };

            HttpResponseMessage response = await client.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Sora API request failed with status code {StatusCode}: {ResponseBody}", response.StatusCode, await response.Content.ReadAsStringAsync(cancellationToken));
                throw new HttpRequestException($"Sora create API request failed with status code {response.StatusCode}");
            }

            string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            using JsonDocument document = JsonDocument.Parse(responseContent);
            string? videoId = document.RootElement.GetProperty("id").GetString();
            if (String.IsNullOrWhiteSpace(videoId))
            {
                ApplicationException ex = new("Failed to get video ID from Sora response");
                logger.LogError(ex, "Failed to get video ID from Sora response: {ResponseContent}", responseContent);
                throw ex;
            }

            logger.LogInformation("Sora video generation started with ID: {VideoId}", videoId);

            SoraJobQuery? result = null;
            while (result == null || result.Status != "completed")
            {
                HttpRequestMessage queryRequest = new(HttpMethod.Get, $"videos/{videoId}");
                HttpResponseMessage queryResponse = await client.SendAsync(queryRequest, cancellationToken);
                if (!queryResponse.IsSuccessStatusCode)
                {
                    logger.LogError("Sora API request failed with status code {StatusCode}: {ResponseBody}", queryResponse.StatusCode, await queryResponse.Content.ReadAsStringAsync(cancellationToken));
                    throw new HttpRequestException($"Sora query API request failed with status code {queryResponse.StatusCode}");
                }

                string queryResponseContent = await queryResponse.Content.ReadAsStringAsync(cancellationToken);
                result = JsonSerializer.Deserialize<SoraJobQuery>(queryResponseContent, SerializationOptions.SnakeCase);

                if (result?.Status == "failed")
                {
                    ApplicationException ex = new("Sora video generation failed");
                    logger.LogError(ex, "Sora video generation failed with code {Code}: {ErrorMessage}", result?.Error?.Code, result?.Error?.Message);
                    throw ex;
                }

                if (result?.Status == "cancelled" || cancellationToken.IsCancellationRequested)
                {
                    OperationCanceledException ex = new("Sora video generation was cancelled", cancellationToken);
                    logger.LogInformation(ex, "Sora video generation was cancelled");
                    throw ex;
                }

                logger.LogInformation("Sora video generation status: {Status}, current progress: {Progress}. Checking again in 20 seconds...", result?.Status, result?.Progress);
                await Task.Delay(TimeSpan.FromSeconds(20), cancellationToken);
            }

            logger.LogInformation("Sora video generation completed for ID: {VideoId}. Starting download...", videoId);
            HttpRequestMessage downloadRequest = new(HttpMethod.Get, $"videos/{videoId}/content");
            HttpResponseMessage downloadResponse = await client.SendAsync(downloadRequest, cancellationToken);
            if (!downloadResponse.IsSuccessStatusCode)
            {
                logger.LogError("Sora API request failed with status code {StatusCode}: {ResponseBody}", downloadResponse.StatusCode, await downloadResponse.Content.ReadAsStringAsync(cancellationToken));
                throw new HttpRequestException($"Sora download API request failed with status code {downloadResponse.StatusCode}");
            }

            using Stream video = await downloadResponse.Content.ReadAsStreamAsync(cancellationToken);
            logger.LogInformation("Sora video downloaded for ID {VideoId}", videoId);
            return BinaryData.FromStream(video);
        }
    }
}
