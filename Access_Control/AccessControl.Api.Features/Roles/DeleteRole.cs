using AccessControl.Api.Common;
using AccessControl.Api.Infrastructure.Persistance;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AccessControl.Api.Features.Roles
{
    public class DeleteRoleController() : ApiControllerBase
    {
        [HttpDelete("/api/roles/{id:guid}")]
        public async Task<IResult> DeleteRole(Guid id)
        {
            var result = await Mediator.Send(new DeleteRoleCommand(id));

            return result.Match(
                _ => Results.NoContent(),
                error => Results.BadRequest(error.First().Description));
        }
    }

    public record DeleteRoleCommand(Guid Id) : IRequest<ErrorOr<Success>>;

    public sealed class DeleteRoleCommandHandler(Context context)
        : IRequestHandler<DeleteRoleCommand, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
        {
            var role = await context.Roles.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
            if (role is null)
                return Error.NotFound("Role.NotFound", "Role not found.");

            context.Roles.Remove(role);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Success;
        }
    }
}
