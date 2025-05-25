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
using static AccessControl.Api.Features.Roles.CreateRolesController;

namespace AccessControl.Api.Features.Roles
{
    [Authorize]
    [RoleAuthorize("Admin")]
    [ApiController]
    public class CreateRolesController() : ApiControllerBase
    {
        [HttpPost("/api/roles")]
        public async Task<IResult> CreateRole([FromForm] CreateRoleQuery query)
        {
            var result = await Mediator.Send(query);

            return result.Match(
                id => Results.Ok(id),
                error => Results.BadRequest(error.First().Description));
        }

        public record CreateRoleQuery(string Name, List<string> PermissionNames) : IRequest<ErrorOr<string>>;
    }

    internal sealed class CreateRoleQueryValidator : AbstractValidator<CreateRoleQuery>
    {
        private readonly Context _context;

        public CreateRoleQueryValidator(Context context)
        {
            _context = context;

            RuleFor(role => role.Name)
                .NotEmpty().WithMessage("A role name is required.")
                .MaximumLength(100).WithMessage("Role name can't be longer than 100 characters.")
                .MustAsync(BeUniqueRoleName).WithMessage("Role name must be unique.");

            RuleFor(role => role.PermissionNames)
                .NotEmpty().WithMessage("At least one permission must be assigned.");
        }

        private async Task<bool> BeUniqueRoleName(string name, CancellationToken token)
        {
            return !await _context.Roles.AnyAsync(r => r.Name == name, token);
        }
    }

    public sealed class CreateRoleQueryHandler(Context context) : IRequestHandler<CreateRoleQuery, ErrorOr<string>>
    {
        private readonly Context _context = context;

        public async Task<ErrorOr<string>> Handle(CreateRoleQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var permissions = await _context.Permissions
                    .Where(p => request.PermissionNames.Contains(p.Name))
                    .ToListAsync(cancellationToken);

                if (permissions.Count != request.PermissionNames.Count)
                {
                    return Error.Validation("Some permission IDs are invalid.");
                }

                var role = new Domain.Entities.Role(request.Name, permissions);
                await _context.Roles.AddAsync(role, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                return role.Name;
            }
            catch (Exception ex)
            {
                return Error.Unexpected(ex.Message);
            }
        }
    }
}
