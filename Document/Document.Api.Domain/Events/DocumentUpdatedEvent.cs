using Document.Api.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Document.Api.Domain.Events
{
    public class DocumentUpdatedEvent : DocumentEventBase
    {
        public string? UpdatedDocumentName { get; set; }
        public string? UpdatedDocumentDescription { get; set; }
        public string? UpdatedFileName { get; set; }
        public string? UpdatedContentType { get; set; }
        public long? UpdatedFileLength { get; set; }
        public string? UpdatedFileUrl { get; set; }
        public string[]? UpdatedTags { get; set; } = Array.Empty<string>();
        public Guid UpdatedByUserId { get; set; } = default!;


        public DocumentUpdatedEvent(Guid id, string name, string description, float version, IFormFile file, string fileUrl, Guid userId, string[]? tags = null)
        {
            this.id = id;
            OccurredAt = DateTime.UtcNow;
            UpdatedDocumentName = name;
            UpdatedDocumentDescription = description;
            UpdatedFileName = file.FileName;
            UpdatedContentType = file.ContentType;
            UpdatedFileLength = file.Length;
            UpdatedFileUrl = fileUrl;
            UpdatedByUserId = userId;
            UpdatedTags = tags;
            Version = version;
        }
    }
}
