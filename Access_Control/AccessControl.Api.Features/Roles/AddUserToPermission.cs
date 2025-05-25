using AccessControl.Api.Common;
using AccessControl.Api.Common.Authorization.Requirements;
using AccessControl.Api.Infrastructure.Persistance;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AccessControl.Api.Features.Roles
{
    [Authorize]
    [RoleAuthorize("Admin")]
    public class AddUserToRoleController() : ApiControllerBase
    {
        [HttpGet("/api/roles/{id:guid}")]
        public async Task<IResult> GetRole([FromBody] AddUserToRoleCommand command)
        {
            var result = await Mediator.Send(command);

            return result.Match(
                role => Results.Ok(role),
                error => Results.NotFound(error.First().Description));
        }
    }

    public record AddUserToRoleCommand(Guid UserId, Guid RoleId) : IRequest<ErrorOr<Unit>>;

    public sealed class AddUserToRoleCommandHandler(Context context)
        : IRequestHandler<AddUserToRoleCommand, ErrorOr<Unit>>
    {
        public async Task<ErrorOr<Unit>> Handle(AddUserToRoleCommand request, CancellationToken cancellationToken)
        {
            var role = await context.Roles
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);

            if (role is null)
                return Error.NotFound("Role.NotFound", "Role not found.");

            role.Users.Add(new Domain.Entities.User { Id = request.UserId });
            await context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
