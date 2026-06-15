using System.Text.Json.Serialization;

namespace Niobium.AI.Ecommerce.Contracts
{
    public class ProductScore
    {
        public required string ScoreVersion { get; set; }

        public string? ProductName { get; set; }

        public string? NormalizedProductType { get; set; }

        public double FinalScore { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PriorityBand PriorityBand { get; set; }

        public required Subscores Subscores { get; set; }

        public List<string> CapsApplied { get; set; } = [];

        public List<string> TopReasons { get; set; } = [];

        public List<string> KeyRisks { get; set; } = [];

        public double EvidenceConfidence { get; set; }

        public List<string> SourcesChecked { get; set; } = [];
    }

    public class Subscores
    {
        public double ImpulseFit { get; set; }

        public double AuDemandEvidence { get; set; }

        public double CompetitionAdvantage { get; set; }

        public double PricingHeadroom { get; set; }

        public double CreativeTransfer { get; set; }

        public double OpsSimplicity { get; set; }

        public double ComplianceSafety { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PriorityBand
    {
        TEST_FIRST,
        STRONG_CANDIDATE,
        VIABLE_NOT_FIRST,
        WEAK,
        DO_NOT_TEST
    }
}
