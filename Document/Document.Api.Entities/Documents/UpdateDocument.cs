using Document.Api.Common;
using Document.Api.Common.Authorization.Requirements;
using Document.Api.Common.Interfaces;
using Document.Api.Domain.Events;
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
    public class UpdateDocumentsController(ICurrentUserService userService) : ApiControllerBase
    {
        [HttpPut("/api/documents/{id:guid}")]
        public async Task<IResult> UploadDocument(
            [FromRoute] Guid id,
            [FromForm] string name,
            [FromForm] string description,
            [FromForm] float version,
            [FromForm] IFormFile file)
        {
            var document = await Mediator.Send(new GetDocumentByIdQuery(id));
            if (document.Value.UserId != userService.UserId)
                return Results.BadRequest("You are not allowed to delete this document.");

            var query = new UpdateDocumentQuery(id, name, description, version, file);
            var result = await Mediator.Send(query);

            return result.Match(
                id => Results.NoContent(),
                error => Results.BadRequest(error.First().Description));
        }
    }

    public record UpdateDocumentQuery(Guid Id, string Name, string Description, float Version, IFormFile File) : IRequest<ErrorOr<Guid>>;

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


    public sealed class UpdateDocumentQueryHandler(IDocumentStorage storage, ICurrentUserService userService) : IRequestHandler<UpdateDocumentQuery, ErrorOr<Guid>>
    {
        private readonly IDocumentStorage _storage = storage;
        private readonly ICurrentUserService _userService = userService;

        public async Task<ErrorOr<Guid>> Handle(UpdateDocumentQuery request, CancellationToken cancellationToken)
        {
            var e = new DocumentUpdatedEvent(request.Id, request.Name, request.Description, request.Version, request.File, "", _userService.UserId);

            if (await _storage.AddDocument(e))
                return e.Id;
            return Error.Failure("something went wrong trying so save the file.");
        }
    }
}
