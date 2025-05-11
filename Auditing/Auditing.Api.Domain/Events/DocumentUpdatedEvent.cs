using Auditing.Api.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auditing.Api.Domain.Events
{
    public class AuditingUpdatedEvent : IAuditingEvent
    {
        public Guid Id { get; set; }
        public DateTime OccurredAt { get; set; }
        public float? Version { get; set; }
        public string? NewAuditingName { get; set; }
        public string? NewAuditingDescription { get; set; }
        public string? NewFileName { get; set; }
        public string? NewContentType { get; set; }
        public long? NewFileSize { get; set; }
        public string? NewFileUrl { get; set; }
        public string[] UpdatedTags { get; set; } = Array.Empty<string>();
        public Guid UpdatedByUserId { get; set; } = default!;

        public AuditingUpdatedEvent(Guid id, string name, string description, float version, IFormFile file, string fileUrl, Guid userId, string[]? tags = null)
        {
            Id = id;
            OccurredAt = DateTime.UtcNow;
            NewAuditingName = name;
            NewAuditingDescription = description;
            NewFileName = file.FileName;
            NewContentType = file.ContentType;
            NewFileSize = file.Length;
            NewFileUrl = fileUrl;
            UpdatedByUserId = userId;
            UpdatedTags = tags;
            Version = version;
        }
    }
}
