using Niobium.AI.Ecommerce.Contracts.ProductDiscovery;

namespace Niobium.AI.Ecommerce.Contracts.CompetitorAnalysis
{
    internal class CompetitorAnalysisOutput
    {
        public required string SourceCountry { get; set; }

        public required string TargetCountry { get; set; }

        public required string Keyword { get; set; }

        public required CompetitorProduct Product { get; set; }

        public CompetitionSignal? CompetitionSignal { get; set; }
    }
}
