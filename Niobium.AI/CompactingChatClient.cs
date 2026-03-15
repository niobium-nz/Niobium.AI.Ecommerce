using Microsoft.Extensions.AI;

namespace Niobium.AI
{
    public class CompactingChatClient(
        IChatClient innerClient, 
        int maxMessagesBeforeCompaction = -1, 
        int trailingMessagesToKeep = -1) 
        : DelegatingChatClient(innerClient)
    {
        public const int DefaultMaxMessagesBeforeCompaction = 12;
        public const int DefaultTrailingMessagesToKeep = 6;

        protected int MaxMessagesBeforeCompaction { get; } = maxMessagesBeforeCompaction >= 0 ? maxMessagesBeforeCompaction : DefaultMaxMessagesBeforeCompaction;

        protected int TrailingMessagesToKeep { get; } = trailingMessagesToKeep >= 0 ? trailingMessagesToKeep : DefaultTrailingMessagesToKeep;

        public override Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
            => base.GetResponseAsync(Compact(messages), options, cancellationToken);

        public override IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
            => base.GetStreamingResponseAsync(Compact(messages), options, cancellationToken);

        private IReadOnlyList<ChatMessage> Compact(IEnumerable<ChatMessage> messages)
        {
            ArgumentNullException.ThrowIfNull(messages);

            var materialized = messages as IReadOnlyList<ChatMessage> ?? [.. messages];
            if (materialized.Count <= MaxMessagesBeforeCompaction)
            {
                return materialized;
            }

            var retainedIndexes = new HashSet<int>();

            var firstUserIndex = FindIndex(materialized, static message => message.Role == ChatRole.User);
            if (firstUserIndex >= 0)
            {
                _ = retainedIndexes.Add(firstUserIndex);
            }

            var lastUserIndex = FindLastIndex(materialized, static message => message.Role == ChatRole.User);
            if (lastUserIndex >= 0)
            {
                _ = retainedIndexes.Add(lastUserIndex);
            }

            for (var index = Math.Max(0, materialized.Count - TrailingMessagesToKeep); index < materialized.Count; index++)
            {
                retainedIndexes.Add(index);
            }

            return [.. retainedIndexes.OrderBy(index => index).Select(index => materialized[index])];
        }

        private static int FindIndex(IReadOnlyList<ChatMessage> messages, Func<ChatMessage, bool> predicate)
        {
            for (var index = 0; index < messages.Count; index++)
            {
                if (predicate(messages[index]))
                {
                    return index;
                }
            }

            return -1;
        }

        private static int FindLastIndex(IReadOnlyList<ChatMessage> messages, Func<ChatMessage, bool> predicate)
        {
            for (var index = messages.Count - 1; index >= 0; index--)
            {
                if (predicate(messages[index]))
                {
                    return index;
                }
            }

            return -1;
        }
    }
}
