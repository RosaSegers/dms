namespace DocumentFrontend.Models
{
    public class Document
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public int? Version { get; set; }
        public string FileUrl { get; set; } = default!;
        public string ContentType { get; set; } = default!;
        public long FileSize { get; set; }
        public Guid UserId { get; set; }
        public DateTime UploadedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string[]? Tags { get; set; }
    }
}
