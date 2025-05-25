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


namespace AccessControl.Api.Features.Roles.Assignment
{
    [Authorize]
    [RoleAuthorize("Admin")]
    [Route("api/roles/{roleId}/permissions")]
    public class GetPermissionsForRoleController : ApiControllerBase
    {
        [HttpGet]
        public async Task<IResult> GetPermissionsForRole(Guid roleId)
        {
            var result = await Mediator.Send(new GetPermissionsForRoleQuery(roleId));

            return result.Match(
                permissions => Results.Ok(permissions),
                error => Results.BadRequest(error.First().Description)
            );
        }
    }

    public record GetPermissionsForRoleQuery(Guid RoleId) : IRequest<ErrorOr<List<Domain.Dtos.Permission>>>;

    public sealed class GetPermissionsForRoleQueryHandler : IRequestHandler<GetPermissionsForRoleQuery, ErrorOr<List<Domain.Dtos.Permission>>>
    {
        private readonly Context _context;
        private readonly IMapper _mapper;

        public GetPermissionsForRoleQueryHandler(Context context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ErrorOr<List<Domain.Dtos.Permission>>> Handle(GetPermissionsForRoleQuery request, CancellationToken cancellationToken)
        {
            var role = await _context.Roles.Include(r => r.Permissions)
                                           .FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);

            if (role == null)
                return Error.NotFound("Role not found");

            var permissionsDto = _mapper.Map<List<Domain.Dtos.Permission>>(role.Permissions);

            return permissionsDto;
        }
    }
}
