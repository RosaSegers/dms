using Document.Api.Common.Interfaces;
using Document.Api.Domain.Events;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Document.Api.Infrastructure.Persistance
{
    public class DocumentStorage : IDocumentStorage
    {
        private List<IDocumentEvent> _documents = [];
        private readonly ICacheService _cache;

        public DocumentStorage(ICacheService cache, IConfiguration configuration)
        {
            _cache = cache;
        }

        public async Task<bool> AddDocument(IDocumentEvent document)
        {
            await Task.Delay(100); // Simulate async operation
            if (document == null)
            {
                Console.WriteLine("Document is null, cannot add to storage.");
                return false;
            }
            _documents.Add(document);
            return true;
        }




        public async Task<List<IDocumentEvent>> GetDocumentList()
        {
            foreach (IDocumentEvent document in _documents)
                Console.WriteLine($"Id: {document.Id}, DocumentId: {document.DocumentId}");
            await Task.Delay(100); // Simulate async operation
            return _documents;
        }

        public async Task<List<IDocumentEvent>> GetDocumentById(Guid id)
        {
            foreach(IDocumentEvent document in _documents) 
                Console.WriteLine($"Document {id}, Id: {document.Id}, DocumentId: {document.DocumentId}");

            await Task.Delay(100); // Simulate async operation
            return _documents.Where(doc => doc.DocumentId == id).ToList();
        }
    }
}
