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
    [Route("api/permissions")]
    public class GetPermissionsController : ApiControllerBase
    {
        [HttpGet]
        public async Task<IResult> GetPermissions()
        {
            var result = await Mediator.Send(new GetPermissionsQuery());

            return result.Match(
                permissions => Results.Ok(permissions),
                error => Results.BadRequest(error.First().Description)
            );
        }
    }

    public record GetPermissionsQuery() : IRequest<ErrorOr<List<Domain.Dtos.Permission>>>;

    public sealed class GetPermissionsQueryHandler : IRequestHandler<GetPermissionsQuery, ErrorOr<List<Domain.Dtos.Permission>>>
    {
        private readonly Context _context;
        private readonly IMapper _mapper;

        public GetPermissionsQueryHandler(Context context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ErrorOr<List<Domain.Dtos.Permission>>> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
        {
            var permissions = await _context.Permissions.ToListAsync(cancellationToken);

            if(permissions.Count == 0)
                return new List<Domain.Dtos.Permission>();

            var permissionDtos = _mapper.Map<List<Domain.Dtos.Permission>>(permissions);

            return permissionDtos;
        }
    }
}
