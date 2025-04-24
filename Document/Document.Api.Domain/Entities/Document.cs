using Document.Api.Common.Interfaces;
using Document.Api.Domain.Events;

namespace Document.Api.Domain.Entities
{
    public class Document
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string FileUrl { get; set; } = default!;
        public string ContentType { get; set; } = default!;
        public long FileSize { get; set; }
        public Guid UploadedByUserId { get; set; }
        public DateTime UploadedAt { get; set; }
        public string[]? Tags { get; set; }

        // Event dispatcher
        public void Apply(IDocumentEvent e)
        {
            switch (e)
            {
                case DocumentUploadedEvent evt:
                    Apply(evt);
                    break;
                //case DocumentUpdatedEvent evt:
                //    Apply(evt);
                //    break;
                //case DocumentDeletedEvent evt:
                //    Apply(evt);
                //    break;
                //case DocumentVersionRevertedEvent evt:
                //    Apply(evt);
                //    break;
            }
        }

        private void Apply(DocumentUploadedEvent e)
        {
            Id = e.Id;
            Name = e.DocumentName;
            Description = e.DocumentDescription;
            FileUrl = e.FileUrl;
            ContentType = e.ContentType;
            FileSize = e.FileSize;
            UploadedByUserId = e.UploadedByUserId;
            UploadedAt = e.OccurredAt;
            Tags = e.Tags;
        }
    }
}
