namespace Niobium.Ads.Agents
{
    public class ProductProfilerOutput
    {
        public string? Url { get; set; }

        public string? FinalUrl { get; set; }

        public int? HttpStatus { get; set; }

        public string? RetrievalDateIso { get; set; }

        public ProductInfo? Product { get; set; }

        public VendorInfo? Vendor { get; set; }

        public List<string> Blockers { get; set; } = [];
    }
}
