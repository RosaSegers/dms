using Document.Api.Common;
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
    public class UpdateDocumentsController() : ApiControllerBase
    {
        [HttpPut("/api/documents")]
        public async Task<IResult> UploadDocument([FromForm] UpdateDocumentQuery query)
        {
            var result = await Mediator.Send(query);

            return result.Match(
                id => Results.Ok(id),
                error => Results.BadRequest(error.First().Description));
        }
    }

    public record UpdateDocumentQuery(Guid Id, string Name, string Description, IFormFile File) : IRequest<ErrorOr<Guid>>;

    internal sealed class UpdateDocumentQueryValidator : AbstractValidator<UpdateDocumentQuery>
    {
        private readonly IVirusScanner _scanner;
        private readonly IDocumentStorage _storage;

        public UpdateDocumentQueryValidator(IVirusScanner scanner, IDocumentStorage storage)
        {
            _scanner = scanner;
            _storage = storage;

            RuleFor(x => x.Id)
                .MustAsync(NotBeDeleted).WithMessage(UpdateDocumentQueryValidatorConstants.FILE_DELETED);

            RuleFor(x => x.File)
                .MustAsync(NotBeVirus).WithMessage(UpdateDocumentQueryValidatorConstants.MALICIOUS_FILE);
        }

        private async Task<bool> NotBeVirus(IFormFile file, CancellationToken token) => (await _scanner.ScanFile(file));
        private async Task<bool> NotBeDeleted(Guid id, CancellationToken token)
            => !(await _storage.GetDocumentById(id)).Any(x => x.GetType() == typeof(DocumentDeletedEvent));
    } 

    internal static class UpdateDocumentQueryValidatorConstants
    {
        internal static string FILE_DELETED = "Sorry, the file has previously been deleted so it can't be edited";
        internal static string MALICIOUS_FILE = "Please don't upload malicious files";
    }


    public sealed class UpdateDocumentQueryHandler(IDocumentStorage storage) : IRequestHandler<UpdateDocumentQuery, ErrorOr<Guid>>
    {
        private readonly IDocumentStorage _storage = storage;
        public async Task<ErrorOr<Guid>> Handle(UpdateDocumentQuery request, CancellationToken cancellationToken)
        {
            var e = new DocumentUpdatedEvent(request.Id, request.Name, request.Description, request.File, "");

            if (await _storage.AddDocument(e))
                return e.Id;
            return Error.Failure("something went wrong trying so save the file.");
        }
    }
}
