using System.Text.Json;
using Niobium.AI.Ecommerce.Contracts;

namespace Niobium.AI.Ecommerce.AgentTools
{
    internal class TestAdsLibrary : IMetaAdsLibrary
    {
        private const string TestDataSource = "Niobium.AI.Ecommerce.AgentTools.dog-hair-removal.json";

        public async Task<MetaAdsSearchResponse> SearchAdsAsync(string keyword, Country country, DateOnly? activeSince = null, CancellationToken? cancellationToken = null)
        {
            using var stream = this.GetType().Assembly.GetManifestResourceStream(TestDataSource)!;
            using var reader = new StreamReader(stream);
            var result = await JsonSerializer.DeserializeAsync<MetaAdsSearchResponse>(stream, SerializationOptions.SnakeCase);
            return result!;
        }
    }
}
