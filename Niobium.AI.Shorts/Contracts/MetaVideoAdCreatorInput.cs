using System;
using System.Collections.Generic;

namespace Niobium.AI.Shorts.Contracts
{
    /// <summary>
    /// Input DTO for publishing a video ad to Meta (Facebook/Instagram).
    /// Contains required fields for targeting and asset location plus optional ad-level metadata.
    /// </summary>
    internal record MetaVideoAdCreatorInput
    {
        // Required common inputs
        public required string AdAccountId { get; init; }
        public required string CampaignName { get; init; }
        public required string AdSetName { get; init; }
        public required string VideoUrl { get; init; }

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
