using Document.Api.Common;
using Document.Api.Common.Interfaces;
using Document.Api.Domain.Events;
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
    public class RolebackDocumentsController() : ApiControllerBase
    {
        [HttpPatch("/api/documents/{id:guid}")]
        public async Task<IResult> UploadDocument(
            [FromRoute] Guid id,
            [FromForm] float version)
        {
            var query = new RolebackDocumentQuery(id, version);
            var result = await Mediator.Send(query);

            return result.Match(
                id => Results.NoContent(),
                error => Results.BadRequest(error.First().Description));
        }
    }

    public record RolebackDocumentQuery(Guid Id, float Version) : IRequest<ErrorOr<Guid>>;

    internal sealed class RolebackDocumentQueryValidator : AbstractValidator<UpdateDocumentQuery>
    {
        private readonly IVirusScanner _scanner;
        private readonly IDocumentStorage _storage;

        public RolebackDocumentQueryValidator(IVirusScanner scanner, IDocumentStorage storage)
        {
            _scanner = scanner;
            _storage = storage;

            RuleFor(x => x.Id)
                .MustAsync(NotBeDeleted).WithMessage(RolebackDocumentQueryValidatorConstants.FILE_DELETED);
        }

        private async Task<bool> NotBeDeleted(Guid id, CancellationToken token)
            => !(await _storage.GetDocumentById(id)).Any(x => x.GetType() == typeof(DocumentDeletedEvent));
    }

    internal static class RolebackDocumentQueryValidatorConstants
    {
        internal static string FILE_DELETED = "Sorry, the file has previously been deleted so it can't be edited";
    }


    public sealed class RolebackDocumentQueryHandler(IDocumentStorage storage, ICurrentUserService userService) : IRequestHandler<RolebackDocumentQuery, ErrorOr<Guid>>
    {
        private readonly IDocumentStorage _storage = storage;
        private readonly ICurrentUserService _userService = userService;

        public async Task<ErrorOr<Guid>> Handle(RolebackDocumentQuery request, CancellationToken cancellationToken)
        {
            var roleback = (await _storage.GetDocumentById(request.Id)).Where(x => x.Version <= request.Version).ToList();
            var e = new DocumentRolebackEvent(request.Id, request.Version, _userService.UserId, roleback);

            if (await _storage.AddDocument(e))
                return e.Id;
            return Error.Failure("something went wrong trying so save the file.");
        }
    }
}
