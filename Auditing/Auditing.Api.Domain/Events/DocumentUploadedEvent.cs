using Microsoft.AspNetCore.Http;
using Auditing.Api.Common.Interfaces;

namespace Auditing.Api.Domain.Events
{
    public class AuditingUploadedEvent : IAuditingEvent
    {
        public Guid Id { get; set; }
        public float? Version { get; set; }
        public string AuditingName { get; set; }
        public string AuditingDescription { get; set; }
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public string ContentType { get; set; }
        public long FileSize { get; set; }
        public Guid UploadedByUserId { get; set; }
        public DateTime OccurredAt { get; set; }
        public string[]? Tags { get; set; }

        public AuditingUploadedEvent(string AuditingName, string AuditingDescription, float version, IFormFile file, string fileUrl, Guid userId, string[]? tags = null)
        {
            Id = Guid.NewGuid();
            AuditingName = AuditingName;
            AuditingDescription = AuditingDescription;
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
