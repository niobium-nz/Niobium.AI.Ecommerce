using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Niobium.AI.OpenAI
{
    internal sealed class KeywordRetryChatClient(IChatClient innerClient, OpenAIOptions retryOptions, ILogger logger) : IChatClient, IDisposable
    {
        public async Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            for (int attempt = 0; ; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ChatResponse response = await innerClient
                    .GetResponseAsync(CloneMessages(messages), options, cancellationToken)
                    .ConfigureAwait(false);

                string responseText = response.Text;
                IEnumerable<AIContent> responseContents = response.Messages.SelectMany(m => m.Contents);
                if (!ShouldRetry(attempt, retryOptions, responseText, responseContents))
                {
                    if (attempt >= retryOptions.MaxRetries)
                    {
                        logger.LogError("Max retry attempts reached for chat response. Returning last error response in anyway.");
                    }

                    return response;
                }

                TimeSpan retryDelay = GetRetryDelay(attempt, retryOptions);
                logger.LogWarning("Retrying chat response... attempt {Attempt}/{MaxRetries} after {RetryDelay}.", attempt + 1, retryOptions.MaxRetries, retryDelay);
                await Task.Delay(retryDelay, cancellationToken).ConfigureAwait(false);
            }
        }

        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
            => this.GetStreamingResponseAsyncCore(messages, options, cancellationToken);

        public object? GetService(Type serviceType, object? serviceKey = null)
        {
            ArgumentNullException.ThrowIfNull(serviceType);
            return innerClient.GetService(serviceType, serviceKey);
        }

        public void Dispose() => innerClient.Dispose();

        private async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsyncCore(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            for (int attempt = 0; ; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                List<ChatResponseUpdate> bufferedUpdates = [];
                StringBuilder responseTextBuilder = new();

                await foreach (ChatResponseUpdate update in innerClient
                    .GetStreamingResponseAsync(CloneMessages(messages), options, cancellationToken)
                    .WithCancellation(cancellationToken)
                    .ConfigureAwait(false))
                {
                    if (!String.IsNullOrEmpty(update.Text))
                    {
                        responseTextBuilder.Append(update.Text);
                    }

                    bufferedUpdates.Add(update);
                }

                string responseText = responseTextBuilder.ToString();
                IEnumerable<AIContent> responseContents = bufferedUpdates.SelectMany(u => u.Contents);
                if (ShouldRetry(attempt, retryOptions, responseText, responseContents))
                {
                    TimeSpan retryDelay = GetRetryDelay(attempt, retryOptions);
                    logger.LogWarning("Retrying streaming chat response... attempt {Attempt}/{MaxRetries} after {RetryDelay}.", attempt + 1, retryOptions.MaxRetries, retryDelay);
                    await Task.Delay(retryDelay, cancellationToken).ConfigureAwait(false);
                    continue;
                }

                if (attempt >= retryOptions.MaxRetries)
                {
                    logger.LogError("Max retry attempts reached for chat response. Returning last error response in anyway.");
                }

                foreach (ChatResponseUpdate update in bufferedUpdates)
                {
                    yield return update;
                }

                yield break;
            }
        }

        private static IEnumerable<ChatMessage> CloneMessages(IEnumerable<ChatMessage> messages)
            => [.. messages.Select(static message => message.Clone())];

        private static TimeSpan GetRetryDelay(int attempt, OpenAIOptions options)
        {
            double backoffMultiplier = Math.Max(1d, options.RetryBackoffMultiplier);
            double exponentialMultiplier = Math.Pow(backoffMultiplier, attempt);
            double delayMs = options.RetryDelay.TotalMilliseconds * exponentialMultiplier;

            if (options.MaxRetryDelay.HasValue)
            {
                delayMs = Math.Min(delayMs, options.MaxRetryDelay.Value.TotalMilliseconds);
            }

            if (options.RetryJitterFactor > 0d)
            {
                double jitterFactor = Math.Clamp(options.RetryJitterFactor, 0d, 1d);
                double jitterScale = 1d + (RandomNumberGenerator.GetInt32(-1000, 1001) / 1000d * jitterFactor);
                delayMs *= jitterScale;
            }

            return TimeSpan.FromMilliseconds(Math.Max(0d, delayMs));
        }

        private static bool ShouldRetry(
            int attempt,
            OpenAIOptions options,
            string responseText,
            IEnumerable<AIContent> responseContents)
            => attempt < options.MaxRetries
                && options.RetryKeywords.Any(keyword => responseText.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                && responseContents.Any(content => content is ErrorContent ec && ec.ErrorCode == "too_many_requests");

    }
}