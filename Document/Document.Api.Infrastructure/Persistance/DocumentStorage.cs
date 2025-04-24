using Document.Api.Domain.Events;

namespace Document.Api.Infrastructure.Persistance
{
    public class DocumentStorage
    {
        private List<DocumentUploadedEvent> documentList = new List<DocumentUploadedEvent>();

        public async Task<bool> AddDocument(DocumentUploadedEvent document)
        {
            documentList.Add(document);
            return true;
        }

        public async Task<List<DocumentUploadedEvent>> GetDocumentList()
        {
            return documentList;
        }
    }
}
