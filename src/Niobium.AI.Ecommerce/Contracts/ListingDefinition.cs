namespace Niobium.AI.Ecommerce.Contracts
{
    internal record ListingDefinition
    {
        public string? ShortProductName { get; init; } = null;
        public required VendorIntegration VendorIntegration { get; init; }
        public required BrandSystem BrandSystem { get; init; }
        public required TrackingSpec TrackingSpec { get; init; }
        public required TrustSignal TrustSignal { get; init; }
    }

    internal record VendorIntegration
    {
        public required Guid TenantId { get; init; }
        public required string GoogleRecaptchaSiteKey { get; init; }
        public required string StripePublicKey { get; init; }
        public required int ShippingOptionId { get; init; }
        public required string FallbackCoupon { get; init; }
        public required string StoreIntegrationEndpoint { get; init; }
        public required string NotificationIntegrationEndpoint { get; init; }
    }

    internal record BrandSystem
    {
        public required string BrandName { get; init; }
        public required string Description { get; init; }
        public required string LogoFile { get; set; }
        public string? PrimaryColor { get; set; }
        public string? SecondaryColor { get; set; }
        public string? AccentColor { get; set; }
        public string? FontStrategy { get; set; }
    }

    internal record TrackingSpec
    {
        public required string MetaPixelId { get; init; }
        public required string Ga4Id { get; init; }
        public required string MicrosoftClarity { get; init; }
        public string[] TrackEvents { get; init; } = [];
        public string[] PreserveQueryParams { get; init; } = [];
    }
}
