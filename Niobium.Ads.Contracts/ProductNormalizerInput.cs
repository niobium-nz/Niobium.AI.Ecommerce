namespace Niobium.Ads
{
    public class ProductNormalizerInput
    {
        public required string ProductName { get; set; }

        public string? CategoryName { get; set; }

        public List<string> KnownFeatures { get; set; } = [];
    }
}
