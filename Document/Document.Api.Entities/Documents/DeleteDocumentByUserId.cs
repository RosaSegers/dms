using Document.Api.Common.Interfaces;
using Document.Api.Domain.Events;
using Document.Api.Infrastructure.Persistance.Interface;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Hosting;
using static Document.Api.Infrastructure.Services.DocumentApiSaga;

namespace Document.Api.Features.Documents;

public sealed class DeleteDocumentByUserIdCommandHandler : IRequestHandler<DeleteDocumentByUserIdCommand, ErrorOr<Unit>>
{
    private readonly IDocumentStorage _storage;
    private readonly IBlobStorageService? _blobStorage;
    private readonly IHostEnvironment _env;

    public DeleteDocumentByUserIdCommandHandler(
        IDocumentStorage storage,
        IBlobStorageService? blobStorage,
        IHostEnvironment env)
    {
        _storage = storage;
        _blobStorage = blobStorage;
        _env = env;
    }

    public async Task<ErrorOr<Unit>> Handle(DeleteDocumentByUserIdCommand request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[DeleteCommandHandler] Deleting documents for user: {request.UserId}");

        var allEvents = await _storage.GetDocumentList();

        if (allEvents is null || allEvents.Count == 0)
            return Unit.Value;

        var eventsByDoc = allEvents.GroupBy(e => e.DocumentId).ToList();

        Console.WriteLine($"[DeleteCommandHandler] Fetched {allEvents.Count} events grouped into {eventsByDoc.Count} documents");

        var documents = new List<Domain.Entities.Document>();

        foreach (var group in eventsByDoc)
        {
            if (group.Any(x => x is DocumentDeletedEvent))
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
                if (document.UserId == request.UserId)
                {
                    if (!_env.IsEnvironment("Test") && _blobStorage is not null)
                        await _blobStorage.DeletePrefixAsync($"{document.Id}/");

                    await _storage.DeleteDocument(document.Id);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DeleteCommandHandler] Failed to delete document {document.Id}: {ex.Message}");
                // Optional: logging, re-queueing, etc.
            }
        }

        Console.WriteLine($"[DeleteCommandHandler] Finished deleting {documents.Count} documents for user: {request.UserId}");

        return Unit.Value;
    }
}
