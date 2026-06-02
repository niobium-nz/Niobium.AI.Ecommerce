using System.Text.Json.Serialization;

namespace Niobium.AI.Ecommerce.Contracts
{
    public class MetaAdsSearchResponse
    {
        public bool Success { get; set; }

        public int CreditsRemaining { get; set; }

        [JsonPropertyName("searchResultsCount")]
        public int SearchResultsCount { get; set; }

        public string? Cursor { get; set; }

        [JsonPropertyName("searchResults")]
        public List<MetaAd> SearchResults { get; set; } = [];
    }
}
