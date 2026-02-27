namespace Niobium.Ads
{
    public class VendorInfo
    {
        public string? BrandName { get; set; }

        public string? Domain { get; set; }

        public string? PlatformDetected { get; set; }

        public PolicyInfo? PolicyUrls { get; set; }

        public List<TrustSignal> TrustSignals { get; set; } = [];
    }
}
