using Document.Api.Common.Interfaces;
using Document.Api.Domain.Events;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace Document.Api.Infrastructure.Persistance
{
    public class DocumentStorage(ICacheService cache) : IDocumentStorage
    {
        private List<IDocumentEvent> documentList = new List<IDocumentEvent>();
        private readonly ICacheService _cache = cache;

        public async Task<bool> AddDocument(IDocumentEvent document)
        {
            documentList.Add(document);
            _cache.InvalidateCaches();

            await Task.Delay(TimeSpan.FromMilliseconds(5));

            return true;
        }

        public async Task<List<IDocumentEvent>> GetDocumentList()
        {
            await Task.Delay(TimeSpan.FromMilliseconds(5));

            return documentList;
        }

        public async Task<List<IDocumentEvent>> GetDocumentById(Guid id)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(5));

            return documentList.Where(x => x.Id == id).ToList();
        }
    }
}
