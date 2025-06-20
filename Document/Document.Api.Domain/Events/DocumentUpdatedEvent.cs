﻿using Document.Api.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
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
        public string[]? UpdatedTags { get; set; } = Array.Empty<string>();
        public Guid UpdatedByUserId { get; set; } = default!;
        [JsonProperty]
        public override string EventType => nameof(DocumentUpdatedEvent);



        public DocumentUpdatedEvent(Guid id, string name, string description, int version, IFormFile file, string fileUrl, Guid userId, string[]? tags = null)
        {
            this.DocumentId = id;
            OccurredAt = DateTime.UtcNow;
            UpdatedDocumentName = name;
            UpdatedDocumentDescription = description;
            UpdatedFileName = file.FileName;
            UpdatedContentType = file.ContentType;
            UpdatedFileLength = file.Length;
            UpdatedByUserId = userId;
            UpdatedTags = tags;
            Version = version;
        }

        public DocumentUpdatedEvent(Guid documentId, DateTime date)
        {
            this.DocumentId = documentId;
            this.OccurredAt = date;
        }
}
}
