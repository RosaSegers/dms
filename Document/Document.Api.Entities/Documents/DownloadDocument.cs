using Document.Api.Common;
using Document.Api.Common.Interfaces;
using Document.Api.Infrastructure.Persistance.Interface;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Azure.Cosmos.Linq;

namespace Document.Api.Features.Documents
{
    [Authorize]
    //[RoleAuthorize("User")]
    public class DownloadDocumentController() : ApiControllerBase
    {
        [HttpGet("/api/documents/{Id}/download")]
        public async Task<IResult> DownloadDocument([FromRoute] Guid id, [FromForm] int? Version = null)
        {
            var document = await Mediator.Send(new GetDocumentByIdQuery(id));

            var result = await Mediator.Send(new DownloadDocumentQuery(
                id, 
                document.Value.Name, 
                Version
                    ?? document.Value.Version
                    ?? throw new ArgumentNullException("Document version could not be found.")));

            return result.Match(
                fileResult => Results.Ok(File(fileResult.FileStream, document.Value.ContentType ?? "application/octet-stream", document.Value.Name)),
                error => Results.BadRequest(error.First().Description));
        }
    }

    public record DownloadDocumentQuery(Guid Id, string DocumentName, int Version) : IRequest<ErrorOr<DownloadDocumentResult>>;

    public record DownloadDocumentResult(Stream FileStream);


    public sealed class DownloadDocumentQueryHandler(IBlobStorageService storage) : IRequestHandler<DownloadDocumentQuery, ErrorOr<DownloadDocumentResult>>
    {
        public async Task<ErrorOr<DownloadDocumentResult>> Handle(DownloadDocumentQuery request, CancellationToken cancellationToken)
        {
            var blobName = $"{request.Id}/{request.DocumentName}_V{request.Version}";

            Console.WriteLine($"[DownloadDocumentQueryHandler] Attempting to download blob with name: {blobName}");

            try
            {
                var stream = await storage.DownloadAsync(blobName);

                return new DownloadDocumentResult(stream);
            }
            catch (FileNotFoundException)
            {
                return Error.NotFound("Blob.NotFound", "The document file was not found.");
            }
        }
    }
}
