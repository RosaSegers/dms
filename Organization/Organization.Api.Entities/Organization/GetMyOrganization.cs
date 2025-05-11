using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Organization.Api.Common;
using Organization.Api.Common.Interfaces;
using Organization.Api.Infrastructure.Persistance;

namespace Organization.Api.Features.Organization
{
    public class GetMyOrganizationsController : ApiControllerBase
    {
        [HttpGet("/api/organization/me")]
        public async Task<IResult> GetMyOrganization()
        {
            var result = await Mediator.Send(new GetMyOrganizationQuery());

            return result.Match(
                org => Results.Ok(org),
                error => error.First().Type == ErrorType.NotFound
                    ? Results.NotFound(error.First().Description)
                    : Results.Problem(error.First().Description)
            );
        }
    }

    public record GetMyOrganizationQuery : IRequest<ErrorOr<OrganizationDto>>;

    public record OrganizationDto(Guid Id, string Name, Guid OwnerId);

    public class GetMyOrganizationQueryHandler(
        DatabaseContext context,
        ICurrentUserService currentUser
    ) : IRequestHandler<GetMyOrganizationQuery, ErrorOr<OrganizationDto>>
    {
        public async Task<ErrorOr<OrganizationDto>> Handle(GetMyOrganizationQuery request, CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(currentUser.UserId);
            if (userId == Guid.Empty)
                return Error.Unauthorized("Unauthorized");

            // Try to find as owner
            var org = await context.Organizations
                .FirstOrDefaultAsync(o => o.OwnerId == userId, cancellationToken);

            if (org is null)
                return Error.NotFound("Organization not found");

            return new OrganizationDto(org.Id, org.Name, org.OwnerId);
        }
    }
}
