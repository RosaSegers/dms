using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Organization.Api.Common;
using Organization.Api.Common.Interfaces;
using Organization.Api.Infrastructure.Persistance;

namespace User.Api.Features.Users
{
    public class UpdateOrganizationController : ApiControllerBase
    {
        [HttpDelete("/api/organization")]
        public async Task<IResult> UpdateOrganization([FromBody] UpdateOrganizationQuery query)
        {
            var result = await Mediator.Send(query);

            return result.Match(
                _ => Results.NoContent(),
                error => error.First().Type == ErrorType.NotFound
                    ? Results.NotFound(error.First().Description)
                    : Results.Problem(error.First().Description)
            );
        }
    }

    public record UpdateOrganizationQuery(Organization.Api.Domain.Entities.Organization org) : IRequest<ErrorOr<Unit>>;


    public class UpdateOrganizationQueryHandler(
        DatabaseContext context,
        ICurrentUserService currentUser
    ) : IRequestHandler<UpdateOrganizationQuery, ErrorOr<Unit>>
    {
        public async Task<ErrorOr<Unit>> Handle(UpdateOrganizationQuery request, CancellationToken cancellationToken)
        {
            var Organization = await context.Organizations.FirstOrDefaultAsync(x => x.Id == request.org.Id, cancellationToken);

            if (Organization == null)
                return Error.NotFound("The organization was not found.");

            context.Organizations.Remove(Organization);
            await context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
