using AccessControl.Api.Common;
using AccessControl.Api.Common.Authorization.Requirements;
using AccessControl.Api.Infrastructure.Persistance;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AccessControl.Api.Features.Roles.Assignment
{
    [Authorize]
    [RoleAuthorize("Admin")]
    [Route("api/roles/{roleId}/permissions")]
    public class AssignPermissionsToRoleController : ApiControllerBase
    {
        [HttpPost]
        public async Task<IResult> AssignPermissionsToRole(Guid roleId, [FromBody] AssignPermissionsToRoleCommand command)
        {
            if (roleId != command.RoleId)
                return Results.BadRequest("Mismatched role ID.");

            var result = await Mediator.Send(command);

            return result.Match(
                _ => Results.NoContent(),
                error => Results.BadRequest(error.First().Description)
            );
        }
    }

    public record AssignPermissionsToRoleCommand(Guid RoleId, List<string> PermissionNames) : IRequest<ErrorOr<Success>>;

    public sealed class AssignPermissionsToRoleCommandValidator : AbstractValidator<AssignPermissionsToRoleCommand>
    {
        public AssignPermissionsToRoleCommandValidator()
        {
            RuleFor(x => x.PermissionNames)
                .NotEmpty().WithMessage("Permissions list cannot be empty.")
                .Must(PermissionNames => PermissionNames.All(name => name.IsNullOrEmpty()))
                .WithMessage("All permission IDs must be valid.");
        }
    }

    public sealed class AssignPermissionsToRoleCommandHandler : IRequestHandler<AssignPermissionsToRoleCommand, ErrorOr<Success>>
    {
        private readonly Context _context;

        public AssignPermissionsToRoleCommandHandler(Context context)
        {
            _context = context;
        }

        public async Task<ErrorOr<Success>> Handle(AssignPermissionsToRoleCommand request, CancellationToken cancellationToken)
        {
            var role = await _context.Roles.Include(r => r.Permissions).FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);
            if (role == null)
                return Error.NotFound("Role not found");

            var permissions = await _context.Permissions.Where(p => request.PermissionNames.Contains(p.Name)).ToListAsync(cancellationToken);
            if (permissions.Count != request.PermissionNames.Count)
                return Error.NotFound("One or more permissions not found");

            role.Permissions.AddRange(permissions);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success;
        }
    }
}
