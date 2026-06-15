using System.Text.Json;
using Microsoft.DurableTask;
using Niobium.AI.Ecommerce.Contracts;

namespace Niobium.AI.Ecommerce.Workflows
{
    [DurableTask]
    internal class PublishCandidate : TaskActivity<ProductDiscoveryOutput, Guid?>
    {
        public override async Task<Guid?> RunAsync(TaskActivityContext context, ProductDiscoveryOutput input)
        {
            bool newPublish = true;
            string candidatePath = $"/artifacts/candidates/{input.CandidateId}.json";
            string candidateDir = Path.GetDirectoryName(candidatePath)!;
            if (!Directory.Exists(candidateDir))
            {
                Directory.CreateDirectory(candidateDir);
            }

            string indexDir = $"/artifacts/candidates/index";
            if (!Directory.Exists(indexDir))
            {
                Directory.CreateDirectory(indexDir);
            }

            string newCandidateId = input.CandidateId.ToString();
            IEnumerable<string> adArchiveIds = input.Ads.Where(ad => !String.IsNullOrWhiteSpace(ad.AdArchiveId)).Select(ad => ad.AdArchiveId!).Distinct();
            foreach (string? adArchiveId in adArchiveIds)
            {
                string indexFile = Path.Combine(indexDir, $"{adArchiveId}.txt");
                if (File.Exists(indexFile))
                {
                    string[] existingCandidates = await File.ReadAllLinesAsync(indexFile);
                    if (!existingCandidates.Contains(newCandidateId))
                    {
                        await File.WriteAllLinesAsync(indexFile, existingCandidates.Concat([newCandidateId]));
                    }
                    else
                    {
                        newPublish = false;
                    }
                }
                else
                {
                    await File.WriteAllTextAsync(indexFile, newCandidateId);
                }
            }

            string json = JsonSerializer.Serialize(input, SerializationOptions.SnakeCase);
            await File.WriteAllTextAsync(candidatePath, json);
            return newPublish ? input.CandidateId : null;
        }
    }
}
