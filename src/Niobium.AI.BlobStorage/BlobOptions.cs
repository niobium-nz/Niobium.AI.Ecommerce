namespace Niobium.AI.BlobStorage
{
    public record BlobOptions
    {
        public required string ControlEndpoint { get; init; }

        public required string DataEndpoint { get; init; }

        public required string AccessToken { get; init; }

        public required string ContainerName { get; init; }
    }
}
