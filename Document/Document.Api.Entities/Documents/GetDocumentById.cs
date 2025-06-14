using Document.Api.Common;
using Document.Api.Common.Authorization.Requirements;
using Document.Api.Common.Constants;
using Document.Api.Common.Interfaces;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Document.Api.Features.Documents
{
    [Authorize]
    //[RoleAuthorize("User")]
    public class GetDocumentByIdController() : ApiControllerBase
    {
        [HttpGet("/api/documents/{Id}")]
        public async Task<IResult> GetDocumentsUsingPagination([FromRoute] Guid Id)
        {
            Console.WriteLine($"When checking id in the controller it is {Id}");

            var result = await Mediator.Send(new GetDocumentByIdQuery(Id));

            return result.Match(
                id => Results.Ok(result.Value),
                error => Results.BadRequest(error.First().Code));
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


    public sealed class GetDocumentByIdQueryHandler(IDocumentStorage storage, ICurrentUserService userService, ICacheService cache) : IRequestHandler<GetDocumentByIdQuery, ErrorOr<Domain.Entities.Document>>
    {
        private readonly IDocumentStorage _storage = storage;
        private readonly ICacheService _cache = cache;

        public async Task<ErrorOr<Domain.Entities.Document>> Handle(GetDocumentByIdQuery request, CancellationToken cancellationToken)
        {
            Console.WriteLine("When checking the id in the request it is: " + request.Id);

            var cacheKey = CacheKeys.GetDocumentCacheKey(request.Id);
            if (_cache.TryGetCache(cacheKey, out object cachedDocument))
            {
                var document = (Domain.Entities.Document?)cachedDocument!;
                if(document == null || document.UserId != userService.UserId)
                    return Error.NotFound("Document not found or you do not have permission to access this document.");

                return (Domain.Entities.Document?)cachedDocument!;
            }

            var events = (await _storage.GetDocumentList()).Where(x => x.DocumentId == request.Id);

            foreach (var e in events)
                Console.WriteLine($"{e.Id} - {e.DocumentId}");

            var doc = new Domain.Entities.Document();
            foreach (var e in events.OrderBy(e => e.OccurredAt))
                doc.Apply(e);

            _cache.SetCache(cacheKey, doc);

            if (doc == null || doc.UserId != userService.UserId)
                return Error.NotFound("Document not found or you do not have permission to access this document.");

            return doc;
        }
    }
}
