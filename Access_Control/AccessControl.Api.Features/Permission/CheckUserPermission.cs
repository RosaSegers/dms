using AccessControl.Api.Common;
using AccessControl.Api.Infrastructure.Persistance;
using AutoMapper;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AccessControl.Api.Features.Permission
{
    [Authorize]
    [Route("api/permissions/{id:guid}")]
    public class CheckUserPermissionController : ApiControllerBase
    {
        [HttpGet]
        public async Task<IResult> GetPermissions(
            [FromBody] string Permission,
            [FromRoute] Guid UserId,
            [FromBody] Guid DocumentId)
        {
            var result = await Mediator.Send(new CheckUserPermissionQuery(Permission, UserId, DocumentId));

            return result.Match(
                permissions => Results.Ok(permissions),
                error => Results.BadRequest(error.First().Description)
            );
        }
    }

    public record CheckUserPermissionQuery(string Permission, Guid UserId, Guid DocumentId) : IRequest<ErrorOr<bool>>;

    public sealed class CheckUserPermissionQueryHandler : IRequestHandler<CheckUserPermissionQuery, ErrorOr<bool>>
    {
        private readonly Context _context;
        private readonly IMapper _mapper;

        public CheckUserPermissionQueryHandler(Context context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ErrorOr<bool>> Handle(CheckUserPermissionQuery request, CancellationToken cancellationToken)
        {
            var roles = await _context.Roles
                .Include(x => x.Permissions)
                .Where(r => r.Users.Any(u => u.Id == request.UserId)).ToListAsync();
            if (roles.Any(r => r.Permissions.Any(p => p.Name == request.Permission)))
                return true;

            return false;
        }
    }
}
