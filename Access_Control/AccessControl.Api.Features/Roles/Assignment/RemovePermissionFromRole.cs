using AccessControl.Api.Common;
using AccessControl.Api.Infrastructure.Persistance;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;


namespace AccessControl.Api.Features.Roles.Assignment
{
    [Route("api/roles/{roleId}/permissions")]
    public class RemovePermissionsFromRoleController : ApiControllerBase
    {
        [HttpDelete]
        public async Task<IResult> RemovePermissionsFromRole(Guid roleId, [FromBody] RemovePermissionsFromRoleCommand command)
        {
            if (roleId != command.RoleId)
                return Results.BadRequest("Mismatched role ID.");

            var result = await Mediator.Send(command);

            return result.Match(
                _ => Results.Ok(),
                error => Results.BadRequest(error.First().Description)
            );
        }
    }

    public record RemovePermissionsFromRoleCommand(Guid RoleId, List<string> PermissionNames) : IRequest<ErrorOr<Success>>;

    public sealed class RemovePermissionsFromRoleCommandValidator : AbstractValidator<RemovePermissionsFromRoleCommand>
    {
        public RemovePermissionsFromRoleCommandValidator()
        {
            RuleFor(x => x.PermissionNames)
                .NotEmpty().WithMessage("Permissions list cannot be empty.")
                .Must(PermissionNames => PermissionNames.All(name => name.IsNullOrEmpty()))
                .WithMessage("All permission IDs must be valid.");
        }
    }

    public sealed class RemovePermissionsFromRoleCommandHandler : IRequestHandler<RemovePermissionsFromRoleCommand, ErrorOr<Success>>
    {
        private readonly Context _context;

        public RemovePermissionsFromRoleCommandHandler(Context context)
        {
            _context = context;
        }

        public async Task<ErrorOr<Success>> Handle(RemovePermissionsFromRoleCommand request, CancellationToken cancellationToken)
        {
            var role = await _context.Roles.Include(r => r.Permissions).FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);
            if (role == null)
                return Error.NotFound("Role not found");

            var permissions = await _context.Permissions.Where(p => request.PermissionNames.Contains(p.Name)).ToListAsync(cancellationToken);

            foreach (var permission in permissions)
            {
                role.Permissions.Remove(permission);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success;
        }
    }
}
