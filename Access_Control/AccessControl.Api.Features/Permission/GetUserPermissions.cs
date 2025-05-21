using AccessControl.Api.Common;
using AccessControl.Api.Common.Interfaces;
using AccessControl.Api.Infrastructure.Persistance;
using AutoMapper;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessControl.Api.Features.Permission
{
    [Route("api/permissions/me")]
    public class GetUserPermissionController : ApiControllerBase
    {
        [HttpGet]
        public async Task<IResult> GetPermissions()
        {
            var result = await Mediator.Send(new GetUserPermissionQuery());

            return result.Match(
                permissions => Results.Ok(permissions),
                error => Results.BadRequest(error.First().Description)
            );
        }
    }

    public record GetUserPermissionQuery() : IRequest<ErrorOr<List<Object>>>;

    public sealed class GetUserPermissionQueryHandler : IRequestHandler<GetUserPermissionQuery, ErrorOr<List<Object>>>
    {
        private readonly Context _context;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _userService;

        public GetUserPermissionQueryHandler(Context context, IMapper mapper, ICurrentUserService userService)
        {
            _context = context;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<ErrorOr<List<Object>>> Handle(GetUserPermissionQuery request, CancellationToken cancellationToken)
        {
            List<Object> permissions = new();
            string userId = _userService.UserId ?? throw new UnauthorizedAccessException();
            Guid user = Guid.Parse(userId);

            permissions.AddRange(await _context.Grants.Where(x => x.UserId == user).ToListAsync(cancellationToken));
            permissions.AddRange(await _context.Assignment.Where(x => x.UserId == user).Include(x => x.Role).ToListAsync(cancellationToken));
            permissions.AddRange(await _context.Roles.Where(x => x.Users.Any(x => x.Id == user)).Include(x => x.Permissions).ToListAsync(cancellationToken));

            return permissions;
        }
    }
}
