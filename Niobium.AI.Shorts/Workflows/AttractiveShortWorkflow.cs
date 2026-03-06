using System.Text.Json;
using Niobium.AI.Shorts.Agents;
using Niobium.AI.Shorts.Contracts;

namespace Niobium.AI.Shorts.Workflows
{
    internal class AttractiveShortWorkflow(
        AttractiveShortDirector attractiveVideoDirector,
        SoraShortProducer soraShortProducer)
        : IWorkflow
    {
        public async Task RunAsync(string conversationID, CancellationToken cancellationToken)
        {
            for (int i = 0; i < 10; i++)
            {
                var story = await attractiveVideoDirector.RunAsync(
                conversationID,
                new AttractiveShortDirectorInput
                {
                    BusinessName = "Beach Front Restaurant and Cocktail Bar",
                    Location = "Mission Bay, Auckland, New Zealand",
                    BusinessType = "Beach Front Restaurant and Cocktail Bar",
                    TypicalSpend = "$60-$80"
                },
                cancellationToken);
                var videoPath = await soraShortProducer.RunAsync(conversationID, new SoraShortProducerInput
                {
                    Prompt = story.VideoPrompt,
                    Width = story.VideoWidth,
                    Height = story.VideoHeight,
                    DurationInSeconds = story.VideoDurationInSeconds,
                }, cancellationToken);
                Console.WriteLine("Generated video URL: " + videoPath);
                await File.WriteAllTextAsync($"{videoPath}.json", JsonSerializer.Serialize(story, SerializationOptions.SnakeCase));
            }
        }
    }
}
