using DocumentFrontend.Components.Pages;
using DocumentFrontend.Models;

namespace DocumentFrontend.Services
{
    public class DocumentService(IHttpClientFactory clientFactory)
    {
        public async Task<PaginatedList<Document>> GetDocumentsAsync()
        {
            var client = clientFactory.CreateClient("Authenticated");

            var response = await client.GetAsync("gateway/documents");

            var responseText = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Login response: {responseText}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<PaginatedList<Document>>();
            }

            return null;
        }

        public async Task<Document?> GetDocumentByIdAsync(Guid id)
        {
            var client = clientFactory.CreateClient("Authenticated");

            var response = await client.GetAsync($"gateway/documents/{id}");

            var responseText = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Login response: {responseText}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Document>();
            }

            return null;
        }

        public Task<Guid> AddDocumentAsync(Document document)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateDocumentAsync(Document updatedDoc)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteDocumentAsync(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
