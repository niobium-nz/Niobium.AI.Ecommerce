namespace Niobium.AI.Ecommerce
{
    public class EcommerceOptions
    {
        public static readonly string SectionName = nameof(EcommerceOptions).ToUpperInvariant();

        public required string ScrapeCreatorsKey { get; set; }
    }
}
