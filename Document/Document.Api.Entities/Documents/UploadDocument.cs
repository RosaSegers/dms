using Document.Api.Common;
using Document.Api.Common.Interfaces;
using Document.Api.Domain.Events;
using Document.Api.Infrastructure.Persistance;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Document.Api.Features.Documents
{
    public class UploadDocumentsController() : ApiControllerBase
    {
        [HttpPost("/api/documents")]
        public async Task<IResult> UploadDocument([FromForm] UploadDocumentQuery query)
        {
            var result = await Mediator.Send(query);

            return result.Match(
                id => Results.Ok(id),
                error => Results.BadRequest(error.First().Description));
        }
    }

    public record UploadDocumentQuery(string Name, string Description, IFormFile File) : IRequest<ErrorOr<Guid>>;

    internal sealed class UploadDocumentQueryValidator : AbstractValidator<UploadDocumentQuery>
    {
        private readonly IVirusScanner _scanner;
        private readonly DocumentStorage _storage;

        public UploadDocumentQueryValidator(IVirusScanner scanner, DocumentStorage storage)
        {
            _scanner = scanner;
            _storage = storage;

            RuleFor(x => x.File)
                .MustAsync(NotBeVirus).WithMessage(UploadDocumentQueryValidatorConstants.MALICIOUS_FILE);
        }

        // add file unique checks
        private async Task<bool> NotBeVirus(IFormFile file, CancellationToken token) => (await _scanner.ScanFile(file));
    }

    internal static class UploadDocumentQueryValidatorConstants
    {
        internal static string MALICIOUS_FILE = "Please don't upload malicious files";
    }


    public sealed class UploadDocumentQueryHandler(DocumentStorage storage) : IRequestHandler<UploadDocumentQuery, ErrorOr<Guid>>
    {
        private readonly DocumentStorage _storage = storage;
        public async Task<ErrorOr<Guid>> Handle(UploadDocumentQuery request, CancellationToken cancellationToken)
        {
            var e = new DocumentUploadedEvent(request.Name, request.Description, request.File, "");

            if(await _storage.AddDocument(e))
                return e.Id;
            return Error.Failure("something went wrong trying so save the file.");
        }
    }
}
