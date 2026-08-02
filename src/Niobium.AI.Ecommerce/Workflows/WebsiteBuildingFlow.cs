using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using Niobium.AI.Ecommerce.Agents;
using Niobium.AI.Ecommerce.Contracts;
using Niobium.AI.Ecommerce.Tools;

namespace Niobium.AI.Ecommerce.Workflows
{
    [DurableTask]
    internal partial class WebsiteBuildingFlow : TaskOrchestrator<WebsiteBuildingInput, WebsiteBuildingOutput>
    {
        public override async Task<WebsiteBuildingOutput> RunAsync(TaskOrchestrationContext context, WebsiteBuildingInput input)
        {
            ILogger logger = context.CreateReplaySafeLogger<WebsiteBuildingFlow>();
            WebsiteBuildingOutput result = await context.CallActivityAsync<WebsiteBuildingOutput>(nameof(FormWebsiteInstruction), input);

            IResponseGenerator<ReviewSimulatorInput, ReviewSimulatorOutput> reviewSimulator = context.GetAgent<ReviewSimulator, ReviewSimulatorInput, ReviewSimulatorOutput>();
            ReviewSimulatorOutput reviews = await reviewSimulator.RunAsync(new ReviewSimulatorInput
            {
                CustomerSegment = result.CustomerSegment,
                ProductDetails = result.ProductDetails,
                TargetCountry = result.TargetCountry,
            });
            if (reviews.Count == 0)
            {
                throw new InvalidOperationException("No reviews generated. Ending workflow.");
            }

            IReadOnlyList<FirstNameCityPair> nameCityPairs = await context.CallActivityAsync<IReadOnlyList<FirstNameCityPair>>(
                nameof(GeneratePersonalData),
                new PersonalDataRequest(result.TargetCountry, reviews.Count));
            for (int i = 0; i < reviews.Count; i++)
            {
                string txt = reviews[i].ReviewText
                    .Replace("Fictional customer-voice simulation for internal use", "")
                    .Replace("Fictional internal review draft", "")
                    .Replace("Fictional internal-use draft", "")
                    .Replace("Fictional internal-use-only draft", "")
                    .Replace("Fictional internal-use-only review draft", "")
                    .Replace("Internal-use-only fictional feedback", "")
                    .Replace("Internal-use-only fictional review draft", "")
                    .Replace("Simulated customer voice draft", "")
                    .Replace("Simulated internal review draft", "")
                    .Trim();
                if (txt.StartsWith(":"))
                {
                    txt = txt[1..].Trim();
                }
                if (txt.StartsWith("[]"))
                {
                    txt = txt[2..].Trim();
                }

                if (txt.Contains("fiction", StringComparison.OrdinalIgnoreCase)
                    || txt.Contains("internal", StringComparison.OrdinalIgnoreCase))
                {
                    logger.LogWarning("Review text contains 'fiction' or 'internal': {ReviewText}", txt);
                }

                result.TrustSignal.Testimonials.Add(new()
                {
                    Testimonial = txt,
                    City = nameCityPairs[i].City,
                    Name = nameCityPairs[i].FirstName,
                });
            }

            result.ShortProductName = result.ShortProductName.ToLowerInvariant();

            string artifactName = $"listings/{input.SignalId}/{input.CandidateId}.json";
            await context.CallActivityAsync(nameof(PublishArtifact), new PublishArtifactInput(artifactName, result, result.GetType()));
            logger.LogInformation("Published website building instruction to artifact storage with name: {ArtifactName}", artifactName);
            return result;
        }
    }
}
