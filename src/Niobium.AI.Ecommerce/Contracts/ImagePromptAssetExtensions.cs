namespace Niobium.AI.Ecommerce.Contracts
{
    internal static class ImagePromptAssetExtensions
    {
        public static ImageForm ToImageForm(this ImagePromptAsset input)
        {
            if (input.Orientation.Equals("portrait", StringComparison.InvariantCultureIgnoreCase)
                || input.Orientation.Equals("vertical", StringComparison.InvariantCultureIgnoreCase))
            {
                return ImageForm.Portrait;
            }
            else if (input.Orientation.Equals("landscape", StringComparison.InvariantCultureIgnoreCase)
                || input.Orientation.Equals("horizontal", StringComparison.InvariantCultureIgnoreCase))
            {
                return ImageForm.Landscape;
            }
            else
            {
                return ImageForm.Square;
            }
        }
    }
}
