using System.Text.Json;
using Microsoft.DurableTask;
using Niobium.AI.Ecommerce.Contracts;

namespace Niobium.AI.Ecommerce.Workflows
{
    [DurableTask]
    internal class PublishSignal : TaskActivity<ProductDiscoveryOutput, Guid?>
    {
        public override async Task<Guid?> RunAsync(TaskActivityContext context, ProductDiscoveryOutput input)
        {
            bool newPublish = true;
            string signalPath = $"/artifacts/signal/{input.SignalId}.json";
            string signalDir = Path.GetDirectoryName(signalPath)!;
            if (!Directory.Exists(signalDir))
            {
                Directory.CreateDirectory(signalDir);
            }

            string indexDir = $"/artifacts/signal/index";
            if (!Directory.Exists(indexDir))
            {
                Directory.CreateDirectory(indexDir);
            }

            string newSignalId = input.SignalId.ToString();
            IEnumerable<string> adArchiveIds = input.Ads.Where(ad => !String.IsNullOrWhiteSpace(ad.AdArchiveId)).Select(ad => ad.AdArchiveId!).Distinct();
            foreach (string? adArchiveId in adArchiveIds)
            {
                string indexFile = Path.Combine(indexDir, $"{adArchiveId}.txt");
                if (File.Exists(indexFile))
                {
                    string[] existingSignals = await File.ReadAllLinesAsync(indexFile);
                    if (!existingSignals.Contains(newSignalId))
                    {
                        await File.WriteAllLinesAsync(indexFile, existingSignals.Concat([newSignalId]));
                    }
                    else
                    {
                        newPublish = false;
                    }
                }
                else
                {
                    await File.WriteAllTextAsync(indexFile, newSignalId);
                }
            }

            string json = JsonSerializer.Serialize(input, SerializationOptions.SnakeCase);
            await File.WriteAllTextAsync(signalPath, json);
            return newPublish ? input.SignalId : null;
        }
    }
}
