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

        public UploadDocumentQueryValidator(IVirusScanner scanner)
        {
            _scanner = scanner;

            RuleFor(x => x.File)
                .MustAsync(IsNotVirus).WithMessage(UploadDocumentQueryValidatorConstants.MALICIOUS_FILE);
        }

        private async Task<bool> IsNotVirus(IFormFile file, CancellationToken token) => (await _scanner.ScanFile(file));
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
            if(_storage.AddDocument(new Domain.Entities.Document(request.Name, request.Description, request.File.FileName)))
                return Guid.NewGuid();
            return Error.Failure("something went wrong");
        }
    }
}
