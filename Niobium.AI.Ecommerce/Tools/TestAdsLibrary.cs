using System.Runtime.CompilerServices;
using System.Text.Json;
using Niobium.AI.Ecommerce.Contracts;

namespace Niobium.AI.Ecommerce.Tools
{
    internal class TestAdsLibrary : IMetaAdsLibrary
    {
        private static readonly string TestDataSource = $"{typeof(TestAdsLibrary).Namespace}.dog-hair-removal.json";

        public async Task<MetaAdsSearchResponse> SearchAdsAsync(string keyword, Country country, DateOnly? activeSince = null, CancellationToken? cancellationToken = null)
        {
            using Stream stream = this.GetType().Assembly.GetManifestResourceStream(TestDataSource)!;
            using StreamReader reader = new(stream);
            MetaAdsSearchResponse? result = await JsonSerializer.DeserializeAsync<MetaAdsSearchResponse>(stream, SerializationOptions.SnakeCase);
            return result!;
        }
    }
}
