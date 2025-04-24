using Document.Api.Common.Interfaces;
using Document.Api.Domain.Events;

namespace Document.Api.Infrastructure.Persistance
{
    public class DocumentStorage : IDocumentStorage
    {
        private List<IDocumentEvent> documentList = new List<IDocumentEvent>();

        public async Task<bool> AddDocument(IDocumentEvent document)
        {
            documentList.Add(document);
            return true;
        }

        public async Task<List<IDocumentEvent>> GetDocumentList()
        {
            return documentList;
        }

        public async Task<List<IDocumentEvent>> GetDocumentById(Guid id)
        {
            return documentList.Where(x => x.Id == id).ToList();
        }
    }
}
