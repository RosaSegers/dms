using Document.Api.Common;
using Document.Api.Common.Constants;
using Document.Api.Common.Interfaces;
using Document.Api.Domain.Events;
using Document.Api.Infrastructure.Persistance;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Document.Api.Features.Documents
{
    public class GetDocumentByIdController() : ApiControllerBase
    {
        [HttpGet("/api/documents/{Id}")]
        public async Task<IResult> GetDocumentsUsingPagination([FromRoute] GetDocumentByIdQuery query)
        {
            var result = await Mediator.Send(query);

            return result.Match(
                id => Results.Ok(result.Value),
                error => Results.BadRequest(error.First().Description));
        }
    }

    public record GetDocumentByIdQuery(Guid Id) : IRequest<ErrorOr<Domain.Entities.Document>>;

    internal sealed class GetDocumentByIdQueryValidator : AbstractValidator<GetDocumentByIdQuery>
    {
        public GetDocumentByIdQueryValidator()
        {

        }
    }

    internal static class GetDocumentByIdQueryConstants
    {

    }


    public sealed class GetDocumentByIdQueryHandler(IDocumentStorage storage, CacheService cache) : IRequestHandler<GetDocumentByIdQuery, ErrorOr<Domain.Entities.Document>>
    {
        private readonly IDocumentStorage _storage = storage;
        private readonly CacheService _cache = cache;

        public async Task<ErrorOr<Domain.Entities.Document>> Handle(GetDocumentByIdQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = CacheKeys.GetDocumentCacheKey(request.Id);
            if (_cache.TryGetCache(cacheKey, out object cachedDocuments))
            {
                return (Domain.Entities.Document?)cachedDocuments;
            }

            var events = (await _storage.GetDocumentList()).Where(x => x.Id == request.Id);

            var doc = new Domain.Entities.Document();
            foreach (var e in events.OrderBy(e => e.OccurredAt))
                doc.Apply(e);

            _cache.SetCache(cacheKey, doc);

            return doc;
        }
    }
}
