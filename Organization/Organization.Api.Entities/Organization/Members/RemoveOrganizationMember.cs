using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Organization.Api.Common;
using Organization.Api.Common.Interfaces;
using Organization.Api.Infrastructure.Persistance;

namespace Organization.Api.Features.Organization.Members
{
    public class RemoveOrganizationMemberController : ApiControllerBase
    {
        [HttpDelete("/api/organization/member")]
        public async Task<IResult> RemoveOrganizationMember([FromBody] RemoveOrganizationMemberCommand command)
        {
            var result = await Mediator.Send(command);

            return result.Match(
                id => Results.NoContent(),
                error => Results.BadRequest(error.First().Description));
        }
    }

    public record RemoveOrganizationMemberCommand(Guid UserId, Guid OrganizationId) : IRequest<ErrorOr<Unit>>;

    public sealed class RemoveOrganizationMemberCommandHandler(
        DatabaseContext context,
        ICurrentUserService currentUserService
    ) : IRequestHandler<RemoveOrganizationMemberCommand, ErrorOr<Unit>>
    {
        public async Task<ErrorOr<Unit>> Handle(RemoveOrganizationMemberCommand request, CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(currentUserService.UserId);
            if (userId == Guid.Empty)
                return Error.Unauthorized("Missing or invalid user");

            context.Members.Remove(await context.Members.SingleAsync(m => m.UserId == request.UserId 
                && m.OrganizationId == request.OrganizationId, cancellationToken));
            context.SaveChanges();

            return Unit.Value;
        }
    }
}
