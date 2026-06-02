namespace Niobium.AI.Ecommerce.Contracts
{
    public class MetaAd
    {
        public string? AdArchiveId { get; set; }

        public string? CollationId { get; set; }

        public string? PageId { get; set; }

        public MetaAdSnapshot? Snapshot { get; set; }

        public bool IsActive { get; set; }

        public bool HasUserReported { get; set; }

        public bool PageIsDeleted { get; set; }

        public string? PageName { get; set; }

        public List<string> Categories { get; set; } = [];

        public bool ContainsDigitalCreatedMedia { get; set; }

        public long EndDate { get; set; }

        public List<string> PublisherPlatform { get; set; } = [];

        public long StartDate { get; set; }

        public bool ContainsSensitiveContent { get; set; }

        public string? Url { get; set; }

        public string? StartDateString { get; set; }

        public string? EndDateString { get; set; }
    }
}
