namespace Niobium.AI.Shorts.Contracts
{
    /// <summary>
    /// Result contract returned after attempting to publish a Meta video ad.
    /// Serialized JSON must conform to the consumer-facing schema with snake_case names.
    /// </summary>
    internal record MetaVideoAdPublisherOutput
    {
        private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "success",
            "partial",
            "failed"
        };

        /// <summary>
        /// Create a new output instance.
        /// </summary>
        /// <param name="status">One of: "success", "partial", "failed".</param>
        /// <param name="adName">Final ad name used, or null.</param>
        /// <param name="campaignMatched">Exact visible campaign name matched, or null.</param>
        /// <param name="adSetMatched">Exact visible ad set name matched, or null.</param>
        /// <param name="warnings">List of concise factual warnings.</param>
        /// <param name="exactFailureStep">Precise step that failed, or null.</param>
        public MetaVideoAdPublisherOutput(
            string status,
            string? adName = null,
            string? campaignMatched = null,
            string? adSetMatched = null,
            IReadOnlyList<string>? warnings = null,
            string? exactFailureStep = null)
        {
            if (String.IsNullOrWhiteSpace(status))
            {
                throw new ArgumentException("status is required", nameof(status));
            }

            if (!AllowedStatuses.Contains(status))
            {
                throw new ArgumentException($"status must be one of: {String.Join(',', AllowedStatuses)}", nameof(status));
            }

            this.Status = status;
            this.AdName = adName;
            this.CampaignMatched = campaignMatched;
            this.AdSetMatched = adSetMatched;
            this.Warnings = warnings ?? [];
            this.ExactFailureStep = exactFailureStep;
        }

        public string Status { get; init; }

        public string? AdName { get; init; }

        public string? CampaignMatched { get; init; }

        public string? AdSetMatched { get; init; }

        public IReadOnlyList<string> Warnings { get; init; }

        public string? ExactFailureStep { get; init; }
    }
}
