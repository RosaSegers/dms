using System.Collections.Concurrent;
using Document.Api.Infrastructure.Services.Interface;

namespace Document.Api.Infrastructure.Services
{
    public class InMemoryDocumentStatusService : IDocumentStatusService
    {
        private readonly ConcurrentDictionary<Guid, string> _statuses = new();

        public Task<string> GetStatusAsync(Guid documentId)
        {
            if (_statuses.TryGetValue(documentId, out var status))
                return Task.FromResult(status);

            return Task.FromResult("not_found");
        }

        public Task SetStatusAsync(Guid documentId, string status)
        {
            _statuses[documentId] = status;
            return Task.CompletedTask;
        }
    }

}
