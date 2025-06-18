using System.Net.Http.Headers;
using System.Net.Http.Json;
using DocumentFrontend.Models;
using Microsoft.AspNetCore.Components.Forms;

namespace DocumentFrontend.Services
{
    public class DocumentService(IHttpClientFactory clientFactory)
    {
        public async Task<PaginatedList<Document>> GetDocumentsAsync()
        {
            var client = clientFactory.CreateClient("Authenticated");

            var response = await client.GetAsync("gateway/documents");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<PaginatedList<Document>>() ?? new PaginatedList<Document>();
            }

            return new PaginatedList<Document>();
        }

        public async Task<Document?> GetDocumentByIdAsync(Guid id)
        {
            var client = clientFactory.CreateClient("Authenticated");

            var response = await client.GetAsync($"gateway/documents/{id}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Document>();
            }

            return null;
        }

        public async Task<Guid> AddDocumentAsync(Document document, IBrowserFile? file)
        {
            var client = clientFactory.CreateClient("Authenticated");

            using var content = new MultipartFormDataContent();

            content.Add(new StringContent(document.Name), "Name");
            content.Add(new StringContent(document.Description), "Description");
            content.Add(new StringContent(document.Version?.ToString() ?? "1"), "Version");
            content.Add(new StringContent(document.UserId.ToString()), "UserId");

            if (document.Tags != null && document.Tags.Any())
                content.Add(new StringContent(string.Join(",", document.Tags)), "Tags");

            if (file != null)
            {
                var fileStream = file.OpenReadStream(maxAllowedSize: 10_000_000);
                var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

                content.Add(fileContent, "File", file.Name);
            }

            var response = await client.PostAsync("gateway/documents", content);

            if (response.IsSuccessStatusCode)
            {
                Guid createdDoc = await response.Content.ReadFromJsonAsync<Guid>();
                return createdDoc;
            }

            return Guid.Empty;
        }

        public async Task<bool> UpdateDocumentAsync(Document updatedDoc, IBrowserFile? file = null)
        {
            var client = clientFactory.CreateClient("Authenticated");

            HttpContent content;

            if (file != null)
            {
                var form = new MultipartFormDataContent();

                form.Add(new StringContent(updatedDoc.Id.ToString()), "id");
                form.Add(new StringContent(updatedDoc.Name), "name");
                form.Add(new StringContent(updatedDoc.Description), "description");
                form.Add(new StringContent(updatedDoc.Version?.ToString() ?? "1"), "version");
                form.Add(new StringContent(updatedDoc.UserId.ToString()), "userId");
                form.Add(new StringContent(string.Join(",", updatedDoc.Tags ?? [])), "tags");

                var fileContent = new StreamContent(file.OpenReadStream(long.MaxValue));
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                form.Add(fileContent, "file", file.Name);

                content = form;
            }
            else
            {
                content = JsonContent.Create(updatedDoc);
            }

            var response = await client.PutAsync($"gateway/documents/{updatedDoc.Id}", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteDocumentAsync(Guid id)
        {
            var client = clientFactory.CreateClient("Authenticated");
            var response = await client.DeleteAsync($"gateway/documents/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
