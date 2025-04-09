using Document.Api.Common;
using Document.Api.Common.Interfaces;
using Document.Api.Infrastructure.Persistance;
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
        public async Task<IResult> UploadDocument([FromForm] GetDocumentQuery query)
        {
            var result = await Mediator.Send(query);

            return result.Match(
                id => Results.Ok(result.Value),
                error => Results.BadRequest(error.First().Description));
        }
    }

    public record GetDocumentQuery() : IRequest<ErrorOr<List<Domain.Entities.Document>>>;

    internal sealed class GetDocumentQueryValidator : AbstractValidator<GetDocumentQuery>
    {

        public GetDocumentQueryValidator()
        {
        }

    }

    internal static class GetDocumentQueryValidatorConstants
    {
        internal static string MALICIOUS_FILE = "Please don't upload malicious files";
    }


    public sealed class GetDocumentQueryHandler(DocumentStorage storage) : IRequestHandler<GetDocumentQuery, ErrorOr<List<Domain.Entities.Document>>>
    {
        private readonly DocumentStorage _storage = storage;
        public async Task<ErrorOr<List<Domain.Entities.Document>>> Handle(GetDocumentQuery request, CancellationToken cancellationToken)
        {
            return _storage.GetDocumentList();
        }
    }
}
