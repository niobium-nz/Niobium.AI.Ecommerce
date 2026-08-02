namespace Niobium.AI.Ecommerce.Contracts
{
    internal record WebsiteBuildingOutput
    {
        public required string ShortProductName { get; set; }
        public required string TargetCountry { get; init; }
        public required VendorIntegration VendorIntegration { get; init; }
        public required BrandSystem BrandSystem { get; init; }
        public required TrackingSpec TrackingSpec { get; init; }
        public required ProductDetails ProductDetails { get; init; }
        public required PricingEconomicsAndOffers PricingEconomicsAndOffers { get; init; }
        public required MobileFirstLandingPagePlan MobileFirstLandingPagePlan { get; init; }
        public required CustomerSegment CustomerSegment { get; init; }
        public required TrustSignal TrustSignal { get; init; }
        public required Dictionary<string, WebsiteAsset> AssetLibrary { get; init; }
    }

    internal record TrustSignal
    {
        public string? Terms { get; set; }
        public string? ReturnsPolicy { get; set; }
        public string? ShippingPolicy { get; set; }
        public string? PrivacyPolicy { get; set; }
        public string? ContactEmail { get; init; }
        public string? FacebookPage { get; init; }
        public string? InstagramPage { get; init; }
        public List<CustomerTestimonial> Testimonials { get; init; } = [];
    }

    internal record CustomerTestimonial
    {
        public string? Name { get; init; }
        public string? City { get; init; }
        public required string Testimonial { get; init; }
        public string? VideoUrl { get; init; }
        public string? MediaRatio { get; init; }
    }

    internal record WebsiteAsset
    {
        public required string Path { get; init; }
        public string? MediaRatio { get; init; }
    }
}
