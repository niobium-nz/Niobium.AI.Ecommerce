namespace Niobium.Ads;

// POCO/record types for parsing the product normalization JSON response
public record ProductNormalizerOutput
{
    public string? Status { get; init; }
    public string? ProductNameGiven { get; init; }
    public string? CategoryNameProvided { get; init; }
    public List<string> KnownFeaturesProvided { get; init; } = [];

    public NormalizationSection? Normalization { get; init; }
    public CompetitiveSetDefinition? CompetitiveSetDefinition { get; init; }
    public List<ProductInterpretation> ProductInterpretations { get; init; } = [];
    public WebResearch? WebResearch { get; init; }
    public KeywordPlan? KeywordPlan { get; init; }
    public List<string> NotesForDownstreamAgent { get; init; } = [];
}

public record NormalizationSection
{
    public List<string> BrandModelTokens { get; init; } = [];
    public string? BaseCategoryInferred { get; init; }
    public List<string> ObservedFromInput { get; init; } = [];
    public List<string> ObservedFromWeb { get; init; } = [];
    public List<string> Assumptions { get; init; } = [];
    public string? Confidence { get; init; }
}

public record CompetitiveSetDefinition
{
    public string? ArchetypePhrase { get; init; }
    public List<string> ArchetypeExplanation { get; init; } = [];
    public string? FormFactor { get; init; }
    public string? JobToBeDone { get; init; }
    public string? PrimaryTarget { get; init; }
    public List<string> Qualifiers { get; init; } = [];
    public List<string> MustIncludeTokens { get; init; } = [];
    public List<string> NearMissClasses { get; init; } = [];
    public List<string> AvoidTerms { get; init; } = [];
}

public record ProductInterpretation
{
    public string? InterpretedProductType { get; init; }
    public string? InterpretedArchetype { get; init; }
    public List<string> WhyThisInterpretation { get; init; } = [];
    public string? Confidence { get; init; }
}

public record WebResearch
{
    public bool Used { get; init; }
    public List<string> Queries { get; init; } = [];
    public List<string> KeyTakeaways { get; init; } = [];
    public List<string> SourceDomains { get; init; } = [];
}

public record KeywordPlan
{
    public List<string> TierAArchetypeTerms { get; init; } = [];
    public List<string> TierBCloseArchetypeSynonyms { get; init; } = [];
    public List<string> TierCBrandModelTerms { get; init; } = [];
    public List<string> TierDBroadContextTerms { get; init; } = [];
    public List<string> AvoidOrExclusionTerms { get; init; } = [];
    public List<string> RecommendedMcpQueries { get; init; } = [];
}
