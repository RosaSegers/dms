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
    public class DeleteOrganizationsController : ApiControllerBase
    {
        [HttpDelete("/api/organization")]
        public async Task<IResult> DeleteOrganization([FromBody] DeleteOrganizationQuery query)
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

    public record DeleteOrganizationQuery(Guid Id) : IRequest<ErrorOr<Unit>>;
    

    public class DeleteOrganizationQueryHandler(
        DatabaseContext context,
        ICurrentUserService currentUser
    ) : IRequestHandler<DeleteOrganizationQuery, ErrorOr<Unit>>
    {
        public async Task<ErrorOr<Unit>> Handle(DeleteOrganizationQuery request, CancellationToken cancellationToken)
        {
            var Organization = await context.Organizations.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (Organization == null) 
                return Error.NotFound("The organization was not found.");

            context.Organizations.Remove(Organization);
            await context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
