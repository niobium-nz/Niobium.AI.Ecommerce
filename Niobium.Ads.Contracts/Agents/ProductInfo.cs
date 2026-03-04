namespace Niobium.Ads.Agents
{
    public class ProductInfo
    {
        public string? Name { get; set; }

        public double? Price { get; set; }

        public string? Currency { get; set; }

        public List<string> Variants { get; set; } = [];

        public List<string> BundleOffers { get; set; } = [];

        public List<string> KeyClaims { get; set; } = [];

        public List<string> IngredientsOrMaterials { get; set; } = [];

        public List<string> Images { get; set; } = [];

        public List<string> Videos { get; set; } = [];

        public SellingPoint? HowItWins { get; init; }
    }
}
