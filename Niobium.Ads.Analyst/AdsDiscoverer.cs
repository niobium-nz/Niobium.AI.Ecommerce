using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;

namespace Niobium.Ads.Analyst
{
    internal class AdsDiscoverer(HttpClient client) : IAgent<AdsDiscovererInput, AdsDiscovererOutput>
    {
        public string Name => nameof(AdsDiscoverer);

        public async Task<AdsDiscovererOutput> RunAsync(string conversationID, AdsDiscovererInput input, CancellationToken cancellationToken)
        {
            var json = await this.RunAsync(conversationID, JsonSerializer.Serialize(input, SerializationOptions.SnakeCase), cancellationToken)
                ?? throw new AgentException("Received null response from AdsDiscoverer API");
            try
            {
                return JsonSerializer.Deserialize<AdsDiscovererOutput>(json, SerializationOptions.SnakeCase)
                    ?? throw new AgentException($"Failed to deserialize AdsDiscovererOutput from API response: {json}");
            }
            catch (JsonException ex)
            {
                throw new AgentException($"Failed to deserialize AdsDiscovererOutput from API response: {json}", ex);
            }
        }

        public async Task<string> RunAsync(string conversationID, string input, CancellationToken cancellationToken)
        {
            var discovererInput = JsonSerializer.Deserialize<AdsDiscovererInput>(input, SerializationOptions.SnakeCase) ?? throw new ArgumentException("Failed to parse input", nameof(input));

            if (!Country.TryParse(discovererInput.Country, out var country))
            {
                throw new ArgumentException($"Invalid country: {discovererInput.Country}", nameof(input));
            }

            var queryParameters = new Dictionary<string, string?>
            {
                { nameof(AdsDiscovererInput.Keyword), discovererInput.Keyword },
                { nameof(AdsDiscovererInput.Country), country.Alpha2 },
                { "activeSince", DateTime.UtcNow.AddMonths(-3).ToString("yyyy-MM-dd") },
            };

            var requestUri = QueryHelpers.AddQueryString(Endpoints.SearchAds, queryParameters);
            var response = await client.GetAsync(requestUri, cancellationToken);
            return !response.IsSuccessStatusCode
                ? throw new Exception($"Failed to get ads discovery results: {response.StatusCode} - {response.ReasonPhrase}")
                : await response.Content.ReadAsStringAsync(cancellationToken);
        }
    }
}
