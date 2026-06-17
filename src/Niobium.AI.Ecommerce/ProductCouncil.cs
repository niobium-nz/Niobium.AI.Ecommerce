using System.Text.Json;
using Niobium.AI.Ecommerce.Contracts;

namespace Niobium.AI.Ecommerce
{
    public class ProductCouncil
    {
        public static async Task Print()
        {
            string candiatesDir = "/artifacts/candidates";
            IEnumerable<string> files = Directory.GetFiles(candiatesDir, "*.json");
            List<ProductDiscoveryOutput> candidates = [];
            foreach (string? file in files)
            {
                string json = await File.ReadAllTextAsync(file);
                ProductDiscoveryOutput candidate = JsonSerializer.Deserialize<ProductDiscoveryOutput>(json, SerializationOptions.SnakeCase)!;
                candidates.Add(candidate);
            }

            foreach (ProductDiscoveryOutput? candidate in candidates.OrderByDescending(c => c.Score.FinalScore))
            {
                Console.WriteLine($"CandidateId: {candidate.CandidateId}, Name: {candidate.Product.LikelyProductName}, Score: {candidate.Score.FinalScore}, Confidence: {candidate.Score.EvidenceConfidence}");
                foreach (MetaAd ad in candidate.Ads)
                {
                    if (ad.Snapshot != null && !String.IsNullOrWhiteSpace(ad.Snapshot.LinkUrl))
                    {
                        Console.WriteLine(ad.Snapshot.LinkUrl);
                    }
                }
                Console.WriteLine();
            }
        }
    }
}
