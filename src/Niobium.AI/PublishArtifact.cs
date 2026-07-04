using System.Text.Json;
using Microsoft.DurableTask;

namespace Niobium.AI
{
    [DurableTask]
    public class PublishArtifact : TaskActivity<PublishArtifactInput, string>
    {
        public override async Task<string> RunAsync(TaskActivityContext context, PublishArtifactInput input)
        {
            if (String.IsNullOrWhiteSpace(input.Name))
            {
                throw new ArgumentNullException(nameof(input.Name));
            }

            string artifactPath = $"/artifacts/{input.Name}";
            string artifactDir = Path.GetDirectoryName(artifactPath)!;
            if (!Directory.Exists(artifactDir))
            {
                Directory.CreateDirectory(artifactDir);
            }

            Type type = Type.GetType($"{input.ArtifactTypeFullName}, {input.ArtifactTypeAssemblyName}")
                ?? throw new ArgumentNullException(nameof(input.ArtifactTypeFullName));
            string json;
            if (input.Artifact is JsonElement je)
            {
                object? obj = JsonSerializer.Deserialize(je.GetRawText(), type, SerializationOptions.Web);
                json = JsonSerializer.Serialize(obj, type, SerializationOptions.SnakeCase);
            }
            else
            {
                json = JsonSerializer.Serialize(input.Artifact, type, SerializationOptions.SnakeCase);
            }

            await File.WriteAllTextAsync(artifactPath, json);
            return artifactPath;
        }
    }
}
