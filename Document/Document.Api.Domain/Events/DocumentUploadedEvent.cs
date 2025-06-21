using Microsoft.AspNetCore.Http;
using Document.Api.Common.Interfaces;
using Newtonsoft.Json;

namespace Document.Api.Domain.Events
{
    public class DocumentUploadedEvent : DocumentEventBase
    {
        public string DocumentName { get; set; } = string.Empty;
        public string DocumentDescription { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public Guid UploadedByUserId { get; set; }  
        public string[]? Tags { get; set; } = Array.Empty<string>();
        [JsonProperty]
        public override string EventType => nameof(DocumentUploadedEvent);



        public DocumentUploadedEvent(string documentName, string documentDescription, int version, IFormFile file, string fileUrl, Guid userId, string[]? tags = null)
        {
            DocumentId = Guid.NewGuid();
            DocumentName = documentName;
            DocumentDescription = documentDescription;
            FileName = file.FileName;
            FileUrl = fileUrl;
            ContentType = file.ContentType;
            FileSize = file.Length;
            UploadedByUserId = userId;
            OccurredAt = DateTime.UtcNow;
            if(tags is not null)
                Tags = tags;
            else
                Tags = Array.Empty<string>();
            Version = version;
        }

        public DocumentUploadedEvent(Guid documentId, DateTime date)
        {
            this.DocumentId = documentId;
            this.OccurredAt = date;
        }

    }
}
