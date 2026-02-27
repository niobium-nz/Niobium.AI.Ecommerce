namespace Niobium.Ads;

public record CompetitionScoutOutput
{
    public RawAdsDiscovered RawAdsDiscovered { get; init; } = new();
    public ExclusionFiltering ExclusionFiltering { get; init; } = new();
    public CompetitionSignal CompetitionSignal { get; init; } = new();
}

public record RawAdsDiscovered
{
    public bool McpCallMade { get; init; }
    public int RawAdsCount { get; init; } = -1; // -1 means unknown
    public int DistinctAdvertisersCount { get; init; } = -1; // -1 means unknown
    public List<string> NotableRawPatterns { get; init; } = [];
    public List<string> Snippets { get; init; } = [];
    public List<string> Limitations { get; init; } = [];
    public string? McpError { get; init; }
}

public record ExclusionFiltering
{
    public bool ExclusionTermsProvided { get; init; }
    public bool FilteringPossible { get; init; }
    public int ExcludedAdsCount { get; init; } = -1; // -1 means unknown
    public int InScopeAdsCount { get; init; } = -1; // -1 means unknown
    public List<string> TopMatchedExclusionTerms { get; init; } = [];
    public string ScopeNote { get; init; } = String.Empty;
}

public record CompetitionSignal
{
    public int Rating0To10 { get; init; } = -1; // -1 means unknown
    public List<string> EvidenceSignals { get; init; } = [];
    public List<string> Inference { get; init; } = [];
    public int Confidence0To10 { get; init; } = -1; // -1 means unknown
    public string Justification { get; init; } = String.Empty;
    public string OperationalDefinition { get; init; } = String.Empty;
}

