using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Document.Api.Common.Interfaces;
using Document.Api.Domain.Events;
using Document.Api.Infrastructure.Persistance.Interface;
using ErrorOr;
using FluentValidation;
using MediatR;
using static Document.Api.Infrastructure.Services.DocumentApiSaga;

namespace Document.Api.Features.Documents
{
    public sealed class DeleteDocumentByUserIdCommandHandler(IDocumentStorage storage, IBlobStorageService blobStorage) : IRequestHandler<DeleteDocumentByUserIdCommand, ErrorOr<Unit>>
    {
        private readonly IDocumentStorage _storage = storage;
        private readonly IBlobStorageService _blobStorage = blobStorage;

        public async Task<ErrorOr<Unit>> Handle(DeleteDocumentByUserIdCommand request, CancellationToken cancellationToken)
        {
            var documents = new List<Domain.Entities.Document>();
            var events = (await _storage.GetDocumentList()).GroupBy(e => e.DocumentId).ToList();

            foreach (var group in events)
            {
                if (group.Any(x => x.GetType() == typeof(DocumentDeletedEvent)))
                    continue;

                var doc = new Domain.Entities.Document();
                foreach (var e in group.OrderBy(e => e.OccurredAt))
                    doc.Apply(e);

                documents.Add(doc);
            }

            foreach (var document in documents)
            {
                await _blobStorage.DeletePrefixAsync($"{document.Id}/");
                Console.WriteLine($"Blob with id {request.Id} has sucessfully been deleted.");

                await _storage.DeleteDocument(document.Id);
                Console.WriteLine($"Document with id {request.Id} has been deleted.");
            }

            return Unit.Value;

        }
    }
}
