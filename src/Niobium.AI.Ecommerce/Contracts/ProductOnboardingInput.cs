namespace Niobium.AI.Ecommerce.Contracts
{
    internal class ProductOnboardingInput
    {
        public required string SourceCountry { get; set; }

        public required string TargetCountry { get; set; }

        public required string Keyword { get; set; }

        public required CompetingProduct Product { get; set; }

        public required MetaAd Ad { get; set; }

        public required ProductCost Cost { get; set; }

        public required Uri ProductVisual { get; set; }
    }
}
