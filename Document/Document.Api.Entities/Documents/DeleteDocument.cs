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
    public class DeleteDocumentsController() : ApiControllerBase
    {
        [HttpDelete("/api/documents")]
        public async Task<IResult> UploadDocument([FromForm] DeleteDocumentQuery query)
        {
            var result = await Mediator.Send(query);

            return result.Match(
                id => Results.Ok(id),
                error => Results.BadRequest(error.First().Description));
        }
    }

    public record DeleteDocumentQuery(Guid Id) : IRequest<ErrorOr<Guid>>;

    internal sealed class DeleteDocumentQueryValidator : AbstractValidator<DeleteDocumentQuery>
    {
        public DeleteDocumentQueryValidator()
        {
        }
    }

    internal static class DeleteDocumentQueryValidatorConstants
    {
    }


    public sealed class DeleteDocumentQueryHandler(DocumentStorage storage) : IRequestHandler<DeleteDocumentQuery, ErrorOr<Guid>>
    {
        private readonly DocumentStorage _storage = storage;
        public async Task<ErrorOr<Guid>> Handle(DeleteDocumentQuery request, CancellationToken cancellationToken)
        {
            var e = new DocumentDeletedEvent(request.Id, Guid.Empty);

            if (await _storage.AddDocument(e))
                return e.Id;
            return Error.Failure("something went wrong trying so save the file.");
        }
    }
}
