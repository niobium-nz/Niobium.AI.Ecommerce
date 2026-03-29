namespace Niobium.AI.Ecommerce.Contracts.ProductNormalization
{
    public class ProductNormalizerInput
    {
        public required string ProductName { get; set; }

        public required string Country { get; set; }

        public string? CategoryName { get; set; }

        public List<string> KnownFeatures { get; set; } = [];
    }
}
