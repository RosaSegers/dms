using AccessControl.Api.Common;
using AccessControl.Api.Common.Authorization.Requirements;
using AccessControl.Api.Common.Mappers;
using AccessControl.Api.Common.Models;
using AccessControl.Api.Infrastructure.Persistance;
using AutoMapper;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AccessControl.Api.Features.Roles
{
    [Authorize]
    [RoleAuthorize("Admin")]
    [ApiController]
    public class GetRolesController() : ApiControllerBase
    {
        [HttpGet("/api/roles")]
        public async Task<IResult> GetRoles([FromQuery] GetRolesWithPaginationQuery query)
        {
            var result = await Mediator.Send(query);

            return result.Match(
                roles => Results.Ok(roles),
                error => Results.BadRequest(error.First().Description));
        }
    }

    public record GetRolesWithPaginationQuery(int PageNumber = 1, int PageSize = 10)
    : IRequest<ErrorOr<PaginatedList<Domain.Dtos.Role>>>;

    internal sealed class GetRolesWithPaginationQueryValidator : AbstractValidator<GetRolesWithPaginationQuery>
    {
        public GetRolesWithPaginationQueryValidator()
        {
            RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1)
                .WithMessage("PageNumber must be at least 1.");

            RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1)
                .WithMessage("PageSize must be at least 1.");
        }
    }

    public sealed class GetRolesWithPaginationQueryHandler(Context context, IMapper mapper)
    : IRequestHandler<GetRolesWithPaginationQuery, ErrorOr<PaginatedList<Domain.Dtos.Role>>>
    {
        private readonly Context _context = context;
        private readonly IMapper _mapper = mapper;

        public async Task<ErrorOr<PaginatedList<Domain.Dtos.Role>>> Handle(GetRolesWithPaginationQuery request, CancellationToken cancellationToken)
        {
            var entities = await _context.Roles
                .Include(r => r.Permissions)
                .OrderBy(r => r.Name)
                .PaginatedListAsync(request.PageNumber, request.PageSize);

            var dtos = _mapper.Map<List<Domain.Dtos.Role>>(entities.Items) ?? [];

            var result = new PaginatedList<Domain.Dtos.Role>(dtos, entities.TotalCount, request.PageNumber, request.PageSize);

            return result;
        }
    }
}
