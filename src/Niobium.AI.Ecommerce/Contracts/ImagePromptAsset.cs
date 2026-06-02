namespace Niobium.AI.Ecommerce.Contracts;

public record ImagePromptAsset
{
    public string AssetId { get; init; } = String.Empty;

    public string Orientation { get; init; } = String.Empty;

    public string OverlayCopySuggestion { get; init; } = String.Empty;

    public string Prompt { get; init; } = String.Empty;
}
