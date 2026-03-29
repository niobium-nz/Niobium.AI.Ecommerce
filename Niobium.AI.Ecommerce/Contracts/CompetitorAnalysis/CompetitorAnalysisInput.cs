using Niobium.AI.Ecommerce.Contracts.ProductDiscovery;
using Niobium.AI.Ecommerce.Contracts.ProductNormalization;

namespace Niobium.AI.Ecommerce.Contracts.CompetitorAnalysis
{
    internal class CompetitorAnalysisInput
    {
        public required string SourceCountry { get; set; }

        public required string TargetCountry { get; set; }

        public required string Keyword { get; set; }

        public required CompetitorProduct Product { get; set; }

        public required string NormalizedKeyword { get; set; }

        public List<string> AvoidOrExclusionTerms { get; set; } = [];

        public List<string> CompetitorAnalysisNotes { get; set; } = [];

        public List<ProductInterpretation> ProductInterpretations { get; set; } = [];
    }
}
