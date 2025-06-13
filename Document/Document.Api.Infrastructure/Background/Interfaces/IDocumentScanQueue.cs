using Document.Api.Domain.Events;
using Microsoft.AspNetCore.Http;

namespace Document.Api.Infrastructure.Background.Interfaces
{
    public interface IDocumentScanQueue
    {
        void Enqueue(DocumentScanQueueItem item);
        bool TryDequeue(out DocumentScanQueueItem item);
        bool TryPeek(out DocumentScanQueueItem item); // <- added
    }

    public record class DocumentScanQueueItem(DocumentUploadedEvent Document, IFormFile File)
    {
        public DocumentUploadedEvent Document { get; init; } = Document;
        public IFormFile File { get; init; } = File;
    }
}
