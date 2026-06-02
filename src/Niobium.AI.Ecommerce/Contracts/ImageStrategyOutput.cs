namespace Niobium.AI.Ecommerce.Contracts;

public record ImageStrategyOutput
{
    public string Status { get; init; } = String.Empty;

    public List<ImagePromptAsset> ImagePrompts { get; init; } = [];
}
