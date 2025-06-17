using DocumentFrontend.Models;

namespace DocumentFrontend.Services
{
    public class DocumentService
    {
        private readonly List<Document> _dummyDocuments = new()
        {
            new Document
            {
                Id = Guid.NewGuid(),
                Name = "Project Plan",
                Description = "Initial project planning document",
                Version = 1,
                FileUrl = "https://example.com/docs/project-plan.pdf",
                ContentType = "application/pdf",
                FileSize = 204800, // 200 KB
                UserId = Guid.NewGuid(),
                UploadedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-5),
                Tags = new[] { "planning", "project" }
            },
            new Document
            {
                Id = Guid.NewGuid(),
                Name = "Budget Report",
                Description = "Quarterly budget analysis",
                Version = 2,
                FileUrl = "https://example.com/docs/budget-report.xlsx",
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileSize = 512000, // 500 KB
                UserId = Guid.NewGuid(),
                UploadedAt = DateTime.UtcNow.AddDays(-20),
                UpdatedAt = DateTime.UtcNow.AddDays(-15),
                Tags = new[] { "finance", "budget" }
            }
        };

        public Task<List<Document>> GetDocumentsAsync()
        {
            return Task.FromResult(_dummyDocuments);
        }

        public Task<Document?> GetDocumentByIdAsync(Guid id)
        {
            var document = _dummyDocuments.FirstOrDefault(d => d.Id == id);
            return Task.FromResult(document);
        }

        public Task<Guid> AddDocumentAsync(Document document)
        {
            document.Id = Guid.NewGuid();
            document.UploadedAt = DateTime.UtcNow;
            document.UpdatedAt = DateTime.UtcNow;
            _dummyDocuments.Add(document);

            return Task.FromResult(document.Id);
        }

        public Task<bool> UpdateDocumentAsync(Document updatedDoc)
        {
            var index = _dummyDocuments.FindIndex(d => d.Id == updatedDoc.Id);
            if (index == -1) return Task.FromResult(false);

            var existing = _dummyDocuments.FirstOrDefault(d => d.Id == updatedDoc.Id);
            if (existing is null) return Task.FromResult(false);

            existing.Name = updatedDoc.Name;
            existing.Description = updatedDoc.Description;
            existing.Version = updatedDoc.Version;
            existing.FileUrl = updatedDoc.FileUrl;
            existing.ContentType = updatedDoc.ContentType;
            existing.FileSize = updatedDoc.FileSize;
            existing.UserId = updatedDoc.UserId;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.Tags = updatedDoc.Tags;

            _dummyDocuments[index] = updatedDoc;

            return Task.FromResult(true);
        }

        public Task<bool> DeleteDocumentAsync(Guid id)
        {
            var existing = _dummyDocuments.FirstOrDefault(d => d.Id == id);
            if (existing is null) return Task.FromResult(false);

            _dummyDocuments.Remove(existing);
            return Task.FromResult(true);
        }
    }
}
