using AccessControl.Api.Common;
using AccessControl.Api.Common.Authorization.Requirements;
using AccessControl.Api.Infrastructure.Persistance;
using AutoMapper;
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
    public class GetRoleByIdController() : ApiControllerBase
    {
        [HttpGet("/api/roles/{id:guid}")]
        public async Task<IResult> GetRole(Guid id)
        {
            var result = await Mediator.Send(new GetRoleByIdQuery(id));

            return result.Match(
                role => Results.Ok(role),
                error => Results.NotFound(error.First().Description));
        }
    }

    public record GetRoleByIdQuery(Guid Id) : IRequest<ErrorOr<Role>>;

    public sealed class GetRoleByIdQueryHandler(Context context, IMapper mapper)
        : IRequestHandler<GetRoleByIdQuery, ErrorOr<Role>>
    {
        public async Task<ErrorOr<Role>> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
        {
            var role = await context.Roles
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (role is null)
                return Error.NotFound("Role.NotFound", "Role not found.");

            return mapper.Map<Role>(role);
        }
    }
}
