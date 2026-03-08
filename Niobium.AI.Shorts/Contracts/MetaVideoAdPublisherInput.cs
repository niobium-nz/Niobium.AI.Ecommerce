using System;
using System.Collections.Generic;

namespace Niobium.AI.Shorts.Contracts
{
    /// <summary>
    /// Input DTO for publishing a video ad to Meta (Facebook/Instagram).
    /// Contains required fields for targeting and asset location plus optional ad-level metadata.
    /// </summary>
    internal record MetaVideoAdPublisherInput
    {
        /// <summary>
        /// Create a new input instance with required common inputs.
        /// </summary>
        /// <param name="adAccountId">Exact Meta ad account ID to operate in.</param>
        /// <param name="campaignName">Exact visible campaign name.</param>
        /// <param name="adSetName">Exact visible ad set name.</param>
        /// <param name="videoUrl">Direct URL or Meta-accepted URL source for the video asset.</param>
        public MetaVideoAdPublisherInput(string adAccountId, string campaignName, string adSetName, string videoUrl)
        {
            if (string.IsNullOrWhiteSpace(adAccountId))
                throw new ArgumentException("adAccountId is required", nameof(adAccountId));
            if (string.IsNullOrWhiteSpace(campaignName))
                throw new ArgumentException("campaignName is required", nameof(campaignName));
            if (string.IsNullOrWhiteSpace(adSetName))
                throw new ArgumentException("adSetName is required", nameof(adSetName));
            if (string.IsNullOrWhiteSpace(videoUrl))
                throw new ArgumentException("videoUrl is required", nameof(videoUrl));

            AdAccountId = adAccountId;
            CampaignName = campaignName;
            AdSetName = adSetName;
            VideoUrl = videoUrl;
        }

        // Required common inputs
        public string AdAccountId { get; init; }
        public string CampaignName { get; init; }
        public string AdSetName { get; init; }
        public string VideoUrl { get; init; }

        // Optional common inputs
        public string? AdName { get; init; }
        public string? PrimaryText { get; init; }
        public string? Headline { get; init; }
        public string? Description { get; init; }
        public string? CallToAction { get; init; }
        public string? PageId { get; init; }
        public string? PageName { get; init; }
        public string? InstagramAccount { get; init; }
        public string? DestinationUrl { get; init; }
        public string? DisplayLink { get; init; }
        public string? PixelId { get; init; }

        /// <summary>
        /// Optional URL parameters expressed as key/value pairs. Use when calling the ad destination.
        /// </summary>
        public IDictionary<string, string>? UrlParameters { get; init; }

        public string? WebsiteEvent { get; init; }

        /// <summary>
        /// Additional ad-level fields supplied by the caller. This allows passing provider-specific keys
        /// without changing the contract for every new ad property.
        /// </summary>
        public IReadOnlyDictionary<string, object>? AdditionalAdFields { get; init; }
    }
}
