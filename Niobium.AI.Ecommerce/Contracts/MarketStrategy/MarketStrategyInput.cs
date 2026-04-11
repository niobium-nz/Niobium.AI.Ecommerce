namespace Niobium.AI.Ecommerce.Contracts.MarketStrategy;

public record MarketStrategyInput
{
    public required string CompetitorUsedProductName { get; init; }
    public required string COGSPerUnit { get; init; }
    public required string TargetMarketCountry { get; init; }
    public required string ExtraUnitCOGSPerOrder { get; init; }
    public required string SalesTax { get; init; }
    public required string PaymentProcessingFees { get; init; }

    public List<string> CompetitorClaims { get; init; } = [];
    public List<string> IngredientsOrMaterials { get; init; } = [];
    public required SellingPoint CompetitorMarketingHowItWins { get; init; }
}