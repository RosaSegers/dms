using Auditing.Api.Common;
using Auditing.Api.Common.Interfaces;
using Auditing.Api.Domain.Events;
using Auditing.Api.Infrastructure.Persistance;
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
    public class DeleteAuditingsController() : ApiControllerBase
    {
        [HttpDelete("/api/Auditings/{id:guid}")]
        public async Task<IResult> UploadAuditing([FromRoute] DeleteAuditingQuery query)
        {
            var result = await Mediator.Send(query);

            return result.Match(
                id => Results.NoContent(),
                error => Results.BadRequest(error.First().Description));
        }
    }

    public record DeleteAuditingQuery(Guid Id) : IRequest<ErrorOr<Guid>>;

    internal sealed class DeleteAuditingQueryValidator : AbstractValidator<DeleteAuditingQuery>
    {
        public DeleteAuditingQueryValidator()
        {
        }
    }

    internal static class DeleteAuditingQueryValidatorConstants
    {
    }


    public sealed class DeleteAuditingQueryHandler(IAuditingStorage storage, ICurrentUserService userService) : IRequestHandler<DeleteAuditingQuery, ErrorOr<Guid>>
    {
        private readonly IAuditingStorage _storage = storage;
        private readonly ICurrentUserService _userService = userService;

        public async Task<ErrorOr<Guid>> Handle(DeleteAuditingQuery request, CancellationToken cancellationToken)
        {
            var e = new AuditingDeletedEvent(request.Id, _userService.UserId);

            if (await _storage.AddAuditing(e))
                return e.Id;
            return Error.Failure("something went wrong trying so save the file.");
        }
    }
}
