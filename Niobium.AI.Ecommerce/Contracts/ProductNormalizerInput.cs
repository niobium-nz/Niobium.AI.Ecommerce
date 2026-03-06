namespace Niobium.AI.Ecommerce.Contracts
{
    public class ProductNormalizerInput
    {
        public required string ProductName { get; set; }

        public string? CategoryName { get; set; }

        public string? TargetCountry { get; set; }

        public List<string> KnownFeatures { get; set; } = [];
    }
}
