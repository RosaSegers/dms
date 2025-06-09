using Microsoft.AspNetCore.Http;
using Document.Api.Common.Interfaces;
using Newtonsoft.Json;

namespace Document.Api.Domain.Events
{
    public class DocumentUploadedEvent : DocumentEventBase
    {
        public string DocumentName { get; set; }
        public string DocumentDescription { get; set; }
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public string ContentType { get; set; }
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
            Tags = tags;
            Version = version;
        }

    }
}
