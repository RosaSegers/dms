using Document.Api.Common.Interfaces;
using Document.Api.Domain.Events;

namespace Document.Api.Domain.Entities
{
    public class Document
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public int? Version { get; set; }
        public string FileName { get; set; } = default!;
        public string ContentType { get; set; } = default!;
        public long FileSize { get; set; }
        public Guid UserId { get; set; }
        public DateTime UploadedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string[]? Tags { get; set; }

        // Event dispatcher
        public void Apply(IDocumentEvent e)
        {
            switch (e)
            {
                case DocumentUploadedEvent evt:
                    Apply((dynamic)evt);
                    break;
                case DocumentUpdatedEvent evt:
                    Apply((dynamic)evt);
                    break;
                case DocumentDeletedEvent evt:
                    Apply((dynamic)evt);
                    break;
                case DocumentRolebackEvent evt:
                    Apply((dynamic)evt);
                    break;
            }
        }

        private void Apply(DocumentUploadedEvent e)
        {
            Id = e.DocumentId;
            Name = e.DocumentName;
            Description = e.DocumentDescription;
            FileName = e.FileName;
            ContentType = e.ContentType;
            FileSize = e.FileSize;
            UserId = e.UploadedByUserId;
            UploadedAt = e.OccurredAt;
            Tags = e.Tags;
            Version = e.Version;
        }

        private void Apply(DocumentUpdatedEvent e)
        {
            if (!string.IsNullOrEmpty(e.UpdatedDocumentName))
                Name = e.UpdatedDocumentName;

            if (!string.IsNullOrEmpty(e.UpdatedDocumentDescription))
                Description = e.UpdatedDocumentDescription;

            if (!string.IsNullOrEmpty(e.UpdatedContentType))
                ContentType = e.UpdatedContentType;

            if (!string.IsNullOrEmpty(e.UpdatedFileName))
                FileName = e.UpdatedFileName!;

            if (e.UpdatedFileLength.HasValue)
                FileSize = e.UpdatedFileLength.Value;

            if (e.UpdatedTags is { Length: > 0 })
                Tags = e.UpdatedTags;

            UpdatedAt = e.OccurredAt;
            UserId = e.UpdatedByUserId;
            Version = e.Version;
        }

        private void Apply(DocumentDeletedEvent e)
        {
            FileName = "[Deleted]";
            FileSize = 0;
            UpdatedAt = e.OccurredAt;
            UserId = e.DeletedByUserId;
            Version = e.Version;
        }

        private void Apply(DocumentRolebackEvent e)
        {
            Id = default;
            Name = default!;
            Description = default!;
            FileName = default!;
            ContentType = default!;
            FileSize = 0;
            UserId = Guid.Empty;
            UploadedAt = default;
            UpdatedAt = e.OccurredAt;
            Tags = null;
            Version = 0;

            foreach (var pastEvent in e.EventsToReapply.OrderBy(x => x.OccurredAt))
            {
                Apply(pastEvent);
            }

            UserId = e.RolledBackByUserId;
            Version = e.Version;
        }
    }
}
