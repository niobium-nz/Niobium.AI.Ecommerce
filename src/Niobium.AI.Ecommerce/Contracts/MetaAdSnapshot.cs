namespace Niobium.AI.Ecommerce.Contracts
{
    public class MetaAdSnapshot
    {
        public string? PageId { get; set; }

        public string? PageProfileUri { get; set; }

        public string? PageName { get; set; }

        public string? PageProfilePictureUrl { get; set; }

        public string? DisplayFormat { get; set; }

        public List<string> PageCategories { get; set; } = [];

        public int PageLikeCount { get; set; }

        public bool IsReshared { get; set; }

        public MetaAdSnapshotBody? Body { get; set; }

        public string? CtaType { get; set; }

        public string? CtaText { get; set; }

        public string? Caption { get; set; }

        public string? LinkDescription { get; set; }

        public string? LinkUrl { get; set; }

        public string? Title { get; set; }

        public string? OriginalImageUrl { get; set; }

        public string? ResizedImageUrl { get; set; }

        public List<MetaAdImage> Images { get; set; } = [];

        public List<MetaAdVideo> Videos { get; set; } = [];

        public List<MetaAdBody> Cards { get; set; } = [];
    }
}
