namespace Niobium.AI.Ecommerce.Contracts.ProductNormalization
{
    internal class ProductNormalizationOutput : ProductNormalizationInput
    {
        public List<string> NormalizedKeywords { get; set; } = [];

        public List<string> CompetitorAnalysisNotes { get; set; } = [];

        public List<string> AvoidOrExclusionTerms { get; set; } = [];

        public List<ProductInterpretation> ProductInterpretations { get; set; } = [];
    }
}
