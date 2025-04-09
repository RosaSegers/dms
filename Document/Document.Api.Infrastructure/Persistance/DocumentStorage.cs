namespace Document.Api.Infrastructure.Persistance
{
    public class DocumentStorage
    {
        private List<Domain.Entities.Document> documentList = new List<Domain.Entities.Document>();

        public bool AddDocument(Domain.Entities.Document document)
        {
            documentList.Add(document);
            return true;
        }

        public List<Domain.Entities.Document> GetDocumentList()
        {
            return documentList;
        }
    }
}
