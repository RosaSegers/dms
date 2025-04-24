using Document.Api.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Document.Api.Domain.Events
{
    public class DocumentUpdatedEvent : IDocumentEvent
    {
        public Guid Id { get; set; }
        public DateTime OccurredAt { get; set; }
        public string? NewDocumentName { get; set; }
        public string? NewDocumentDescription { get; set; }
        public string? NewFileName { get; set; }
        public string? NewContentType { get; set; }
        public long? NewFileSize { get; set; }
        public string? NewFileUrl { get; set; }
        public string[] UpdatedTags { get; set; } = Array.Empty<string>();
        public Guid UpdatedByUserId { get; set; } = default!;

        public DocumentUpdatedEvent(Guid id, string name, string description, IFormFile file, string fileUrl, Guid? userId = null, string[]? tags = null)
        {
            Id = id;
            OccurredAt = DateTime.UtcNow;
            NewDocumentName = name;
            NewDocumentDescription = description;
            NewFileName = file.FileName;
            NewContentType = file.ContentType;
            NewFileSize = file.Length;
            NewFileUrl = fileUrl;
            UpdatedByUserId = userId??Guid.Empty;
            UpdatedTags = tags;
        }
    }
}
