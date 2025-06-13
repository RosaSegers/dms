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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Document.Api.Features.Documents
{
    [Authorize]
    //[RoleAuthorize("User")]
    public class UploadDocumentsController() : ApiControllerBase
    {
        [HttpPost("/api/documents")]
        public async Task<IResult> UploadDocument([FromForm] UploadDocumentCommand command)
        {
            var result = await Mediator.Send(command);

            return result.Match(
                id => Results.Accepted($"/api/documents/{id}/status", new { DocumentId = id }),
                error => Results.BadRequest(error.First().Description));
        }
    }

    public record UploadDocumentCommand(string Name, string Description, int Version, IFormFile File) : IRequest<ErrorOr<Guid>>;

    public sealed class UploadDocumentCommandHandler(IDocumentScanQueue queue, ICurrentUserService userService)
        : IRequestHandler<UploadDocumentCommand, ErrorOr<Guid>>
    {
        public async Task<ErrorOr<Guid>> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
        {
            var evt = new DocumentUploadedEvent(
                request.Name, request.Description, request.Version,
                request.File, "", userService.UserId
            );

            Console.WriteLine(request.File);

            using var memoryStream = new MemoryStream();
            await request.File.CopyToAsync(memoryStream, cancellationToken);
            memoryStream.Position = 0;
            var item = new DocumentScanQueueItem(
                evt,
                new MemoryStream(memoryStream.ToArray()),
                request.File.FileName,
                request.File.ContentType
            );

            queue.Enqueue(item);


            // You could also persist the "pending" status here in DB
            return await Task.FromResult<ErrorOr<Guid>>(evt.DocumentId);
        }
    }

}
