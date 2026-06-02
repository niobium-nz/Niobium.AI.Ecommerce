namespace Niobium.AI.Ecommerce.Contracts;

public record MarketStrategyInput
{
    public required string CompetitorUsedProductName { get; init; }
    public required double COGSPerUnit { get; init; }
    public required string TargetMarketCountry { get; init; }
    public required double ExtraUnitCOGSPerOrder { get; init; }
    public required string SalesTax { get; init; }
    public required string PaymentProcessingFees { get; init; }
    public List<string> CompetitorClaims { get; init; } = [];
    public List<string> IngredientsOrMaterials { get; init; } = [];
    public SellingPoint? CompetitorMarketingHowItWins { get; init; }
}