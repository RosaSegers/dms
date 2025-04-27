using Microsoft.AspNetCore.Http;
using Document.Api.Common.Interfaces;

namespace Document.Api.Domain.Events
{
    public class DocumentUploadedEvent : IDocumentEvent
    {
        public Guid Id { get; set; }
        public float? Version { get; set; }
        public string DocumentName { get; set; }
        public string DocumentDescription { get; set; }
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public string ContentType { get; set; }
        public long FileSize { get; set; }
        public Guid UploadedByUserId { get; set; }
        public DateTime OccurredAt { get; set; }
        public string[]? Tags { get; set; }

        public DocumentUploadedEvent(string documentName, string documentDescription, float version, IFormFile file, string fileUrl, Guid userId, string[]? tags = null)
        {
            Id = Guid.NewGuid();
            DocumentName = documentName;
            DocumentDescription = documentDescription;
            FileName = file.FileName;
            FileUrl = fileUrl;
            ContentType = file.ContentType;
            FileSize = file.Length;
            UploadedByUserId = userId;
            OccurredAt = DateTime.UtcNow;
            Tags = tags;
            Version = version;
        }

    }
}
