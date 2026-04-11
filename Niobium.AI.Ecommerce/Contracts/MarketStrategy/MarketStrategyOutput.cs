namespace Niobium.AI.Ecommerce.Contracts.MarketStrategy;

public record MarketStrategyOutput
{
    public ProductDetails ProductDetails { get; init; } = new();
    public PricingEconomicsAndOffers PricingEconomicsAndOffers { get; init; } = new();
    public MobileFirstLandingPagePlan MobileFirstLandingPagePlan { get; init; } = new();
    public List<CustomerSegment> CustomerSegments { get; init; } = [];
}

public record ProductDetails
{
    public string WorkingProductDefinition { get; init; } = string.Empty;
    public string CoreProblemSolved { get; init; } = string.Empty;
    public List<string> PrimaryUseCases { get; init; } = [];
    public string MaterialsOrConstructionSummary { get; init; } = string.Empty;
    public string FulfillmentAndRefundAssumptions { get; init; } = string.Empty;
    public List<string> SuggestedProductNames { get; init; } = [];
    public string RecommendedPrimaryProductName { get; init; } = string.Empty;
    public string RecommendedPrimaryProductNameRationale { get; init; } = string.Empty;
}

public record PricingEconomicsAndOffers
{
    public List<string> GivenInputs { get; init; } = [];
    public List<string> ModeledAssumptions { get; init; } = [];
    public string LandedCostModel { get; init; } = string.Empty;
    public string RecommendedSingleUnitSellingPrice { get; init; } = string.Empty;
    public string? OptionalCompareAtOrAnchorPrice { get; init; }
    public DirectPurchaseFunnelModel DirectPurchaseFunnelModel { get; init; } = new();
    public DiagnosticMetrics? DiagnosticMetrics { get; init; }
    public OfferStack OfferStack { get; init; } = new();
    public string BundleRationale { get; init; } = string.Empty;
    public string RecommendedPrimaryOffer { get; init; } = string.Empty;
    public string RecommendedPrimaryOfferRationale { get; init; } = string.Empty;
}

public record DirectPurchaseFunnelModel
{
    public string AssumedCpc { get; init; } = string.Empty;
    public string AssumedLpvRate { get; init; } = string.Empty;
    public string AssumedCheckoutStartRate { get; init; } = string.Empty;
    public string AssumedPurchaseRate { get; init; } = string.Empty;
    public string DerivedClickToPurchaseRate { get; init; } = string.Empty;
    public string EstimatedBlendedCpa { get; init; } = string.Empty;
    public string BreakEvenBlendedCpa { get; init; } = string.Empty;
    public string RecommendedTargetBlendedCpa { get; init; } = string.Empty;
    public string FunnelLogicExplanation { get; init; } = string.Empty;
}

public record DiagnosticMetrics
{
    public string EstimatedCostPerLpv { get; init; } = string.Empty;
    public string EstimatedCostPerCheckoutStart { get; init; } = string.Empty;
}

public record OfferStack
{
    public Offer SingleUnitOffer { get; init; } = new();
    public Offer BestSellerBundle { get; init; } = new();
    public Offer HigherAovBundle { get; init; } = new();
}

public record Offer
{
    public string Name { get; init; } = string.Empty;
    public string PricePoint { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}

public record MobileFirstLandingPagePlan
{
    public string RoleInDirectPurchaseFunnel { get; init; } = string.Empty;
    public List<string> MobileFirstSectionOrder { get; init; } = [];
    public List<LandingPageSectionAssetPlan> CreativeAssetsNeededForEachSection { get; init; } = [];
    public string ProofStrategyWithoutReviewsOrRealSocialProof { get; init; } = string.Empty;
    public string ShippingAndGuaranteeMessagingGuidance { get; init; } = string.Empty;
    public List<string> FaqAndObjectionHandlingPriorities { get; init; } = [];
    public string AlignmentWithCustomerSegmentsAndAngleTriggerMatrix { get; init; } = string.Empty;
    public string MobileUxNotes { get; init; } = string.Empty;
}

public record LandingPageSectionAssetPlan
{
    public string SectionName { get; init; } = string.Empty;
    public List<string> AssetNeeds { get; init; } = [];
}

public record CustomerSegment
{
    public int SegmentNumber { get; init; }
    public string SegmentName { get; init; } = string.Empty;
    public SharedContextSnapshot SharedContextSnapshot { get; init; } = new();
    public SegmentRating SegmentRating { get; init; } = new();
    public SegmentSummary SegmentSummary { get; init; } = new();
    public List<AngleTrigger> AngleAndTriggerMatrix { get; init; } = [];
    public SegmentLandingPageAdaptation SegmentLandingPageAdaptation { get; init; } = new();
}

public record SharedContextSnapshot
{
    public string RecommendedProductName { get; init; } = string.Empty;
    public string CoreOfferStack { get; init; } = string.Empty;
    public List<string> KeyPricePoints { get; init; } = [];
    public string ShippingReality { get; init; } = string.Empty;
    public string GuaranteeOrRefundFraming { get; init; } = string.Empty;
    public string LandingPageRoleInFunnel { get; init; } = string.Empty;
}

public record SegmentRating
{
    public int OverallPriorityRating { get; init; }
    public int ProfitPotentialRating { get; init; }
    public int TriggerIntensityRating { get; init; }
    public int CreativeClarityRating { get; init; }
    public int FunnelFitRating { get; init; }
    public string Rationale { get; init; } = string.Empty;
}

public record SegmentSummary
{
    public string WhoThisSegmentIs { get; init; } = string.Empty;
    public string NeedState { get; init; } = string.Empty;
    public string PurchaseMomentOrTriggerCondition { get; init; } = string.Empty;
    public string EmotionalDriver { get; init; } = string.Empty;
    public List<string> MainObjections { get; init; } = [];
    public string EconomicAttractiveness { get; init; } = string.Empty;
}

public record AngleTrigger
{
    public int AngleNumber { get; init; }
    public string AngleName { get; init; } = string.Empty;
    public string Trigger { get; init; } = string.Empty;
    public string CorePromise { get; init; } = string.Empty;
    public string MessageTerritory { get; init; } = string.Empty;
    public string WhyThisAngleShouldConvertOnMeta { get; init; } = string.Empty;
    public List<string> ObjectionsToPreHandle { get; init; } = [];
    public List<string> ProofOrDemoAssetsRequired { get; init; } = [];
    public string RecommendedCtaOrRoasTarget { get; init; } = string.Empty;
    public CreativeHandoffs CreativeHandoffs { get; init; } = new();
}

public record CreativeHandoffs
{
    public List<string> StaticImageDirections { get; init; } = [];
    public List<string> ShortFormVideoOrUgcDirections { get; init; } = [];
    public string FirstFrameOrThumbStopDirection { get; init; } = string.Empty;
    public List<string> CopyHookTerritories { get; init; } = [];
    public List<string> HeadlineTerritories { get; init; } = [];
    public List<string> VisualProofMoments { get; init; } = [];
    public string ContinuityNotesForLandingPage { get; init; } = string.Empty;
}

public record SegmentLandingPageAdaptation
{
    public string HeroDirection { get; init; } = string.Empty;
    public List<string> BenefitOrder { get; init; } = [];
    public List<string> FaqEmphasis { get; init; } = [];
    public List<string> SectionEmphasis { get; init; } = [];
    public string MobileUxNotes { get; init; } = string.Empty;
    public List<string> AssetNeedsSpecificToThisSegment { get; init; } = [];
}
