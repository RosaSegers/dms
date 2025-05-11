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

namespace Organization.Api.Features.Organization.Invitations
{
    public class AcceptOrganizationInviteController : ApiControllerBase
    {
        [HttpGet]
        public async Task<IResult> AcceptOrganizationInvite([FromBody] AcceptOrganizationInviteCommand command)
        {
            var result = await Mediator.Send(command);

            return result.Match(
                id => Results.NoContent(),
                error => Results.BadRequest(error.First().Description));
        }
    }

    public record AcceptOrganizationInviteCommand(Guid id) : IRequest<ErrorOr<Unit>>;

    public sealed class AcceptOrganizationInviteHandler(
        DatabaseContext context,
        ICurrentUserService currentUserService
    ) : IRequestHandler<AcceptOrganizationInviteCommand, ErrorOr<Unit>>
    {
        public async Task<ErrorOr<Unit>> Handle(AcceptOrganizationInviteCommand request, CancellationToken cancellationToken)
        {
            var invite = await context.Invites.SingleOrDefaultAsync(x => x.Id == request.id, cancellationToken);

            if (invite == null)
                return Error.NotFound($"Item with id of {request.id} was not found.");

            if (invite.ExpiresAt < DateTime.UtcNow)
            {
                invite.Status = Common.Enums.InviteStatus.Expired;
                return Error.Conflict($"The item with id of {request.id} has expired.");
            }

            if (invite.Status == Common.Enums.InviteStatus.Accepted)
                return Error.Conflict($"The invite has already been accepted");

            invite.Status = Common.Enums.InviteStatus.Accepted;
            await context.Members.AddAsync(new Domain.Entities.Member(invite.OrganizationId, Guid.Parse(currentUserService.UserId)));

            context.SaveChanges();

            return Unit.Value;
        }
    }
}
