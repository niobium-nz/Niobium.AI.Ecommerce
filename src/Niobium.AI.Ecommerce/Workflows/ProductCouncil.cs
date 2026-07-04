using System.Text.Json;
using Niobium.AI.Ecommerce.Contracts;

namespace Niobium.AI.Ecommerce.Workflows
{
    public class ProductCouncil
    {
        public static async Task Print()
        {
            string signalDir = "/artifacts/signal";
            IEnumerable<string> files = Directory.GetFiles(signalDir, "*.json");
            List<ProductDiscoveryOutput> signals = [];
            foreach (string? file in files)
            {
                string json = await File.ReadAllTextAsync(file);
                ProductDiscoveryOutput signal = JsonSerializer.Deserialize<ProductDiscoveryOutput>(json, SerializationOptions.SnakeCase)!;
                signals.Add(signal);
            }

            foreach (ProductDiscoveryOutput? signal in signals.OrderByDescending(c => c.Score.FinalScore))
            {
                Console.WriteLine($"SignalId: {signal.SignalId}, Name: {signal.Product.LikelyProductName}, Score: {signal.Score.FinalScore}, Confidence: {signal.Score.EvidenceConfidence}");
                foreach (MetaAd ad in signal.Ads)
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
