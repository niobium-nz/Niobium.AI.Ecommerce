namespace Niobium.AI.Ecommerce.Contracts.ProductProfile
{
    public class ProductProfilerOutput
    {
        public string? Url { get; set; }

        public string? FinalUrl { get; set; }

        public string? RetrievalDateTimeUtc { get; set; }

        public ProductInfo? Product { get; set; }

        public VendorInfo? Vendor { get; set; }

        public List<string> Blockers { get; set; } = [];
    }
}
