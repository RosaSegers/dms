using Document.Api.Common;
using Document.Api.Common.Interfaces;
using Document.Api.Common.Models;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Document.Api.Features.Documents
{
    public class GetDocumentsController() : ApiControllerBase
    {
        [HttpGet("/api/documents")]
        public async Task<IResult> GetDocumentsUsingPagination([FromQuery] GetDocumentsWithPaginationQuery query)
        {
            var result = await Mediator.Send(query);

            return result.Match(
                id => Results.Ok(result.Value),
                error => Results.BadRequest(error.First().Description));
        }
    }

    public record GetDocumentsWithPaginationQuery(int PageNumber = 1, int PageSize = 10) : IRequest<ErrorOr<PaginatedList<Domain.Entities.Document>>>;

    internal sealed class GetDocumentsWithPaginationQueryValidator : AbstractValidator<GetDocumentsWithPaginationQuery>
    {
        public GetDocumentsWithPaginationQueryValidator()
        {
            RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1)
                .WithMessage(GetDocumentsWithPaginationQueryConstants.PAGENUMBER_GREATER_THAN_STRING);

            RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1)
                .WithMessage(GetDocumentsWithPaginationQueryConstants.PAGESIZE_GREATER_THAN_STRING);
        }
    }

    internal static class GetDocumentsWithPaginationQueryConstants
    {
        internal static string PAGENUMBER_GREATER_THAN_STRING = "PageNumber at least greater than or equal to 1.";
        internal static string PAGESIZE_GREATER_THAN_STRING = "PageSize at least greater than or equal to 1.";
    }


    public sealed class GetDocumentsWithPaginationQueryHandler(IDocumentStorage storage) : IRequestHandler<GetDocumentsWithPaginationQuery, ErrorOr<PaginatedList<Domain.Entities.Document>>>
    {
        private readonly IDocumentStorage _storage = storage;
        public async Task<ErrorOr<PaginatedList<Domain.Entities.Document>>> Handle(GetDocumentsWithPaginationQuery request, CancellationToken cancellationToken)
        {
            var documents = new List<Domain.Entities.Document>();
            var events = (await _storage.GetDocumentList()).GroupBy(e => e.Id).ToList();

            foreach (var group in events) 
            {
                var doc = new Domain.Entities.Document();
                foreach (var e in group.OrderBy(e => e.OccurredAt))
                    doc.Apply(e);

                documents.Add(doc);
            }

            var paginatedDocuments = documents
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new PaginatedList<Domain.Entities.Document>(paginatedDocuments, documents.Count, request.PageNumber, request.PageSize);
        }
    }
}
