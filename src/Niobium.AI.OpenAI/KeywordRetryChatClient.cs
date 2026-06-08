using System.Runtime.CompilerServices;
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
            List<ChatMessage> bufferedMessages = [.. messages.Select(static message => message.Clone())];

            for (int attempt = 0; ; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ChatResponse response = await innerClient
                    .GetResponseAsync(CloneMessages(bufferedMessages), options, cancellationToken)
                    .ConfigureAwait(false);

                if (!ShouldRetry(attempt, retryOptions, response.Text))
                {
                    return response;
                }

                logger.LogWarning("Retrying chat response due to presence of retry keywords. Attempt {Attempt}/{MaxRetries}.", attempt + 1, retryOptions.MaxRetries);
                await Task.Delay(retryOptions.RetryDelay, cancellationToken).ConfigureAwait(false);
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
            List<ChatMessage> bufferedMessages = [.. messages.Select(static message => message.Clone())];
            List<ChatResponseUpdate> bufferedUpdates = [];

            for (int attempt = 0; ; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                StringBuilder responseTextBuilder = new();

                await foreach (ChatResponseUpdate update in innerClient
                    .GetStreamingResponseAsync(CloneMessages(bufferedMessages), options, cancellationToken)
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
                if (ShouldRetry(attempt, retryOptions, responseText))
                {
                    logger.LogWarning("Retrying streaming chat response due to presence of retry keywords. Attempt {Attempt}/{MaxRetries}.", attempt + 1, retryOptions.MaxRetries);
                    await Task.Delay(retryOptions.RetryDelay, cancellationToken).ConfigureAwait(false);
                    continue;
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

        private static bool ShouldRetry(
            int attempt,
            OpenAIOptions options,
            string input)
            => attempt < options.MaxRetries && options.RetryKeywords.Any(keyword => input.Contains(keyword, StringComparison.OrdinalIgnoreCase));

    }
}