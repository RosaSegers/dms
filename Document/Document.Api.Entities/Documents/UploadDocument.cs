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
    public class UploadDocumentsController() : ApiControllerBase
    {
        [HttpPost("/api/documents")]
        public async Task<IResult> UploadDocument([FromForm] UploadDocumentQuery query)
        {
            var result = await Mediator.Send(query);

            return result.Match(
                id => Results.Created("/api/documents", id),
                error => Results.BadRequest(error.First().Description));
        }
    }

    public record UploadDocumentQuery(string Name, string Description, float Version, IFormFile File) : IRequest<ErrorOr<Guid>>;

    internal sealed class UploadDocumentQueryValidator : AbstractValidator<UploadDocumentQuery>
    {
        private readonly IVirusScanner _scanner;
        private readonly IDocumentStorage _storage;

        public UploadDocumentQueryValidator(IVirusScanner scanner, IDocumentStorage storage)
        {
            _scanner = scanner;
            _storage = storage;

            RuleFor(x => x.File)
                .MustAsync(NotBeVirus).WithMessage(UploadDocumentQueryValidatorConstants.MALICIOUS_FILE);
        }

        private async Task<bool> NotBeVirus(IFormFile file, CancellationToken token) => (await _scanner.ScanFile(file));
    }

    internal static class UploadDocumentQueryValidatorConstants
    {
        internal static string MALICIOUS_FILE = "Please don't upload malicious files";
    }


    public sealed class UploadDocumentQueryHandler(IDocumentStorage storage, ICurrentUserService userService) : IRequestHandler<UploadDocumentQuery, ErrorOr<Guid>>
    {
        private readonly IDocumentStorage _storage = storage;
        private readonly ICurrentUserService _userService = userService;

        public async Task<ErrorOr<Guid>> Handle(UploadDocumentQuery request, CancellationToken cancellationToken)
        {
            var e = new DocumentUploadedEvent(request.Name, request.Description, request.Version, request.File, "", _userService.UserId);

            if(await _storage.AddDocument(e))
                return e.DocumentId;
            return Error.Failure("something went wrong trying so save the file.");
        }
    }
}
