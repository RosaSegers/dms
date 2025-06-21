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
using Microsoft.IdentityModel.Tokens;
using static Document.Api.Infrastructure.Services.DocumentApiSaga;

namespace Document.Api.Features.Documents
{
    public sealed class ExistsDocumentByUserIdQueryHandler(IDocumentStorage storage, IBlobStorageService blobStorage)
        : IRequestHandler<ExistsDocumentByUserIdQuery, ErrorOr<bool>>
    {
        private readonly IDocumentStorage _storage = storage;
        private readonly IBlobStorageService _blobStorage = blobStorage;

        public async Task<ErrorOr<bool>> Handle(ExistsDocumentByUserIdQuery request, CancellationToken cancellationToken)
        {
            Console.WriteLine($"[ExistsQueryHandler] Checking if documents exist for user: {request.Id}");

            var allEvents = await _storage.GetDocumentList();
            var eventsByDoc = allEvents.GroupBy(e => e.DocumentId).ToList();

            Console.WriteLine($"[ExistsQueryHandler] Fetched {allEvents.Count} events grouped into {eventsByDoc.Count} documents");

            var documents = new List<Domain.Entities.Document>();

            foreach (var group in eventsByDoc)
            {
                if (group.Any(x => x.GetType() == typeof(DocumentDeletedEvent)))
                {
                    Console.WriteLine($"[ExistsQueryHandler] Skipping deleted document: {group.Key}");
                    continue;
                }

                var doc = new Domain.Entities.Document();
                foreach (var e in group.OrderBy(e => e.OccurredAt))
                    doc.Apply(e);

                documents.Add(doc);
            }

            Console.WriteLine($"[ExistsQueryHandler] Active documents found: {documents.Count}");

            return !documents.IsNullOrEmpty();
        }
    }
}
