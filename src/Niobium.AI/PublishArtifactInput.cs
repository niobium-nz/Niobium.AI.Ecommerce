using System.Diagnostics.CodeAnalysis;

namespace Niobium.AI
{
    public class PublishArtifactInput
    {
        public PublishArtifactInput()
        {
        }

        [SetsRequiredMembers]
        public PublishArtifactInput(string name, object artifact, Type artifactType)
        {
            this.Name = name;
            this.Artifact = artifact;
            this.ArtifactTypeFullName = artifactType.FullName ?? throw new ArgumentNullException(nameof(artifactType.FullName));
            this.ArtifactTypeAssemblyName = artifactType.Assembly.GetName().Name ?? throw new ArgumentNullException(nameof(artifactType.Assembly));
        }

        public required string Name { get; init; }
        public required object Artifact { get; init; }
        public required string ArtifactTypeFullName { get; init; }
        public required string ArtifactTypeAssemblyName { get; init; }
    }
}