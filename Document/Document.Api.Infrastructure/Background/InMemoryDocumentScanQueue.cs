using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Document.Api.Domain.Events;
using Document.Api.Infrastructure.Background.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Document.Api.Infrastructure.Background
{
    public class InMemoryDocumentScanQueue : IDocumentScanQueue
    {
        private readonly ConcurrentQueue<DocumentScanQueueItem> _queue = new();

        public void Enqueue(DocumentScanQueueItem item) => _queue.Enqueue(item);

        public bool TryDequeue(out DocumentScanQueueItem item) => _queue.TryDequeue(out item);
    }

}
