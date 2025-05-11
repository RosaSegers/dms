using Auditing.Api.Common;
using Auditing.Api.Common.Interfaces;
using Auditing.Api.Domain.Events;
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

namespace Auditing.Api.Features.Auditings
{
    public class RolebackAuditingsController() : ApiControllerBase
    {
        [HttpPatch("/api/Auditings/{id:guid}")]
        public async Task<IResult> UploadAuditing(
            [FromRoute] Guid id,
            [FromForm] float version)
        {
            var query = new RolebackAuditingQuery(id, version);
            var result = await Mediator.Send(query);

            return result.Match(
                id => Results.NoContent(),
                error => Results.BadRequest(error.First().Description));
        }
    }

    public record RolebackAuditingQuery(Guid Id, float Version) : IRequest<ErrorOr<Guid>>;

    internal sealed class RolebackAuditingQueryValidator : AbstractValidator<UpdateAuditingQuery>
    {
        private readonly IAuditingStorage _storage;

        public RolebackAuditingQueryValidator(IAuditingStorage storage)
        {
            _storage = storage;

            RuleFor(x => x.Id)
                .MustAsync(NotBeDeleted).WithMessage(RolebackAuditingQueryValidatorConstants.FILE_DELETED);
        }

        private async Task<bool> NotBeDeleted(Guid id, CancellationToken token)
            => !(await _storage.GetAuditingById(id)).Any(x => x.GetType() == typeof(AuditingDeletedEvent));
    }

    internal static class RolebackAuditingQueryValidatorConstants
    {
        internal static string FILE_DELETED = "Sorry, the file has previously been deleted so it can't be edited";
    }


    public sealed class RolebackAuditingQueryHandler(IAuditingStorage storage, ICurrentUserService userService) : IRequestHandler<RolebackAuditingQuery, ErrorOr<Guid>>
    {
        private readonly IAuditingStorage _storage = storage;
        private readonly ICurrentUserService _userService = userService;

        public async Task<ErrorOr<Guid>> Handle(RolebackAuditingQuery request, CancellationToken cancellationToken)
        {
            var roleback = (await _storage.GetAuditingById(request.Id)).Where(x => x.Version <= request.Version).ToList();
            var e = new AuditingRolebackEvent(request.Id, request.Version, _userService.UserId, roleback);

            if (await _storage.AddAuditing(e))
                return e.Id;
            return Error.Failure("something went wrong trying so save the file.");
        }
    }
}
