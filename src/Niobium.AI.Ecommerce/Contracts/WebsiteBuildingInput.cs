namespace Niobium.AI.Ecommerce.Contracts
{
    internal record WebsiteBuildingInput
    {
        public required Guid SignalId { get; init; }

        public required Guid CandidateId { get; init; }

        public required string BrandCode { get; init; }

        public required int CustomerSegmentId { get; init; }

        public required int TriggeredAngleId { get; init; }

        public string[] Assets { get; init; } = [];
    }
}