using AccessControl.Api.Common;
using AccessControl.Api.Infrastructure.Persistance;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AccessControl.Api.Features.Roles
{
    public class UpdateRoleController() : ApiControllerBase
    {
        [HttpPut("/api/roles/{id:guid}")]
        public async Task<IResult> UpdateRole(Guid id, [FromForm] UpdateRoleCommand command)
        {
            if (id != command.Id)
                return Results.BadRequest("Mismatched role ID");

            var result = await Mediator.Send(command);

            return result.Match(
                _ => Results.NoContent(),
                error => Results.BadRequest(error.First().Description));
        }
    }

    public record UpdateRoleCommand(Guid Id, string Name, List<string> PermissionNames) : IRequest<ErrorOr<Success>>;

    internal sealed class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
    {
        private readonly Context _context;

        public UpdateRoleCommandValidator(Context context)
        {
            _context = context;

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Role name is required.")
                .MaximumLength(100).WithMessage("Role name must be less than 100 characters.")
                .MustAsync((command, name, cancellationToken) => BeUniqueRoleName(command.Id, name, cancellationToken))
                .WithMessage("Role name must be unique.");

            RuleFor(x => x.PermissionNames)
                .NotEmpty().WithMessage("At least one permission must be assigned.")
                .MustAsync(BeValidPermissions).WithMessage("Some permission IDs are invalid.");
        }

        private async Task<bool> BeUniqueRoleName(Guid roleId, string name, CancellationToken token)
            => !await _context.Roles.AnyAsync(r => r.Name == name && r.Id != roleId, token);

        private async Task<bool> BeValidPermissions(List<string> permissionIds, CancellationToken token)
            => await _context.Permissions.AnyAsync(p => permissionIds.Contains(p.Name), token);
    }

    public sealed class UpdateRoleCommandHandler(Context context)
        : IRequestHandler<UpdateRoleCommand, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
        {
            var role = await context.Roles
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (role is null)
                return Error.NotFound("Role.NotFound", "Role not found.");

            var permissions = await context.Permissions
                .Where(p => request.PermissionNames.Contains(p.Name))
                .ToListAsync(cancellationToken);

            role.Name = request.Name;
            role.Permissions.AddRange(permissions);

            await context.SaveChangesAsync(cancellationToken);
            return Result.Success;
        }
    }
}
