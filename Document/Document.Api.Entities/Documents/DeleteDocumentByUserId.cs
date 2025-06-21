using Document.Api.Common.Interfaces;
using Document.Api.Domain.Events;
using Document.Api.Infrastructure.Persistance.Interface;
using ErrorOr;
using MediatR;
using static Document.Api.Infrastructure.Services.DocumentApiSaga;

namespace Document.Api.Features.Documents
{
#if TEST
    public sealed class DeleteDocumentByUserIdCommandHandler(IDocumentStorage storage)
#else
    public sealed class DeleteDocumentByUserIdCommandHandler(IDocumentStorage storage, IBlobStorageService blobStorage)
#endif
        : IRequestHandler<DeleteDocumentByUserIdCommand, ErrorOr<Unit>>
    {
        private readonly IDocumentStorage _storage = storage;
#if !TEST
        private readonly IBlobStorageService _blobStorage = blobStorage;
#endif

        public async Task<ErrorOr<Unit>> Handle(DeleteDocumentByUserIdCommand request, CancellationToken cancellationToken)
        {
            Console.WriteLine($"[DeleteCommandHandler] Deleting documents for user: {request.UserId}");

            var allEvents = await _storage.GetDocumentList();
            
            if (allEvents is null)
                return Unit.Value;
            if (allEvents.Count == 0)
                return Unit.Value;

            var eventsByDoc = allEvents.GroupBy(e => e.DocumentId).ToList();

            Console.WriteLine($"[DeleteCommandHandler] Fetched {allEvents.Count} events grouped into {eventsByDoc.Count} documents");

            var documents = new List<Domain.Entities.Document>();

            foreach (var group in eventsByDoc)
            {
                if (group.Any(x => x.GetType() == typeof(DocumentDeletedEvent)))
                {
                    Console.WriteLine($"[DeleteCommandHandler] Skipping already-deleted document: {group.Key}");
                    continue;
                }

                var doc = new Domain.Entities.Document();
                foreach (var e in group.OrderBy(e => e.OccurredAt))
                    doc.Apply(e);

                documents.Add(doc);
            }

            foreach (var document in documents)
            {
                Console.WriteLine($"[DeleteCommandHandler] Deleting blob and storage for document: {document.Id}");

                try
                {
                    if(document.UserId == request.UserId)
                    {
#if !TEST
                        await _blobStorage.DeletePrefixAsync($"{document.Id}/");
#endif
                        await _storage.DeleteDocument(document.Id);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DeleteCommandHandler] Failed to delete document {document.Id}: {ex.Message}");
                    // Optionally: log, re-queue, or trigger a failure event
                }

            }

            Console.WriteLine($"[DeleteCommandHandler] Finished deleting {documents.Count} documents for user: {request.UserId}");

            return Unit.Value;
        }
    }

}
