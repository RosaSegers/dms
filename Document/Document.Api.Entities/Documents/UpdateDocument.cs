using Document.Api.Common;
using Document.Api.Common.Authorization.Requirements;
using Document.Api.Common.Interfaces;
using Document.Api.Domain.Events;
using Document.Api.Infrastructure.Background.Interfaces;
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
    public class UpdateDocumentsController(ICurrentUserService userService, IDocumentScanQueue queue) : ApiControllerBase
    {
        [HttpPut("/api/documents/{id:guid}")]
        public async Task<IResult> UploadDocument(
            [FromRoute] Guid id,
            [FromForm] string name,
            [FromForm] string description,
            [FromForm] int version,
            [FromForm] IFormFile file)
        {
            var document = await Mediator.Send(new GetDocumentByIdQuery(id));
            if (document.Value.UserId != userService.UserId)
                return Results.BadRequest("You are not allowed to edit this document.");

            var command = new UpdateDocumentCommand(id, name, description, version, file);
            var result = await Mediator.Send(command);

            return result.Match(
                _ => Results.NoContent(),
                error => Results.BadRequest(error.First().Description));
        }
    }


    public record UpdateDocumentCommand(Guid Id, string Name, string Description, int Version, IFormFile File) : IRequest<ErrorOr<Guid>>;

    internal sealed class UpdateDocumentCommandValidator : AbstractValidator<UpdateDocumentCommand>
    {
        private readonly IDocumentStorage _storage;

        public UpdateDocumentCommandValidator(IDocumentStorage storage)
        {
            _storage = storage;

            RuleFor(x => x.Id)
                .MustAsync(NotBeDeleted).WithMessage(UpdateDocumentCommandValidatorConstants.FILE_DELETED);
        }
        private async Task<bool> NotBeDeleted(Guid id, CancellationToken token)
            => !(await _storage.GetDocumentById(id)).Any(x => x.GetType() == typeof(DocumentDeletedEvent));
    } 

    internal static class UpdateDocumentCommandValidatorConstants
    {
        internal static string FILE_DELETED = "Sorry, the file has previously been deleted so it can't be edited";
    }

    public sealed class UpdateDocumentCommandHandler(IDocumentScanQueue queue, ICurrentUserService userService)
        : IRequestHandler<UpdateDocumentCommand, ErrorOr<Guid>>
    {
        public async Task<ErrorOr<Guid>> Handle(UpdateDocumentCommand request, CancellationToken cancellationToken)
        {
            var evt = new DocumentUpdatedEvent(
                request.Id,
                request.Name,
                request.Description,
                request.Version,
                request.File,
                "",
                userService.UserId
            );

            // Copy stream into memory to allow multiple uses
            await using var memoryStream = new MemoryStream();
            await request.File.CopyToAsync(memoryStream, cancellationToken);
            memoryStream.Position = 0;

            var copyBuffer = memoryStream.ToArray(); // ensure full read
            var streamCopy = new MemoryStream(copyBuffer); // clone for scanning/uploading

            queue.Enqueue(new DocumentScanQueueItem(
                evt,
                streamCopy,
                request.File.FileName,
                request.File.ContentType
            ));

            return evt.DocumentId;
        }
    }

}
