﻿using Document.Api.Common;
using Document.Api.Common.Authorization.Requirements;
using Document.Api.Common.Interfaces;
using Document.Api.Domain.Events;
using Document.Api.Infrastructure.Persistance.Interface;
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
    public class DeleteDocumentsController(ICurrentUserService userService) : ApiControllerBase
    {
        [HttpDelete("/api/documents/{id:guid}")]
        public async Task<IResult> UploadDocument([FromRoute] Guid id)
        {
            var document = await Mediator.Send(new GetDocumentByIdQuery(id));
            if (document.Value.UserId != userService.UserId)
                return Results.BadRequest("You are not allowed to delete this document.");

            var result = await Mediator.Send(new DeleteDocumentCommand(id));

            return result.Match(
                id => Results.NoContent(),
                error => Results.BadRequest(error.First().Description));
        }
    }

    public record DeleteDocumentCommand(Guid Id, string? DocumentName = null) : IRequest<ErrorOr<Unit>>;

    internal sealed class DeleteDocumentCommandValidator : AbstractValidator<DeleteDocumentCommand>
    {
        public DeleteDocumentCommandValidator()
        {
        }
    }

    internal static class DeleteDocumentCommandValidatorConstants
    {
    }


    public sealed class DeleteDocumentCommandHandler(IDocumentStorage storage, ICurrentUserService userService, IBlobStorageService blobStorage) : IRequestHandler<DeleteDocumentCommand, ErrorOr<Unit>>
    {
        private readonly IDocumentStorage _storage = storage;
        private readonly IBlobStorageService _blobStorage = blobStorage;
        private readonly ICurrentUserService _userService = userService;

        public async Task<ErrorOr<Unit>> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
        {
            var e = new DocumentDeletedEvent(request.Id, _userService.UserId);

            if (await _storage.AddDocument(e))
            {
                await _blobStorage.DeletePrefixAsync($"{request.Id}/");

                Console.WriteLine($"Blob with id {request.Id} has sucessfully been deleted.");
                return Unit.Value;
            }

            return Error.Failure("something went wrong trying so save the file.");
        }
    }
}
