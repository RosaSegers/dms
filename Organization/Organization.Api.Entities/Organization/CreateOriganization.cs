using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Organization.Api.Common;
using Organization.Api.Common.Interfaces;
using Organization.Api.Infrastructure.Persistance;

namespace Organization.Api.Features.Organization
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationsController : ApiControllerBase
    {
        [HttpPost]
        public async Task<IResult> CreateOrganization([FromBody] CreateOrganizationCommand command)
        {
            var result = await Mediator.Send(command);

            return result.Match(
                id => Results.NoContent(),
                error => Results.BadRequest(error.First().Description));
        }
    }

    public record CreateOrganizationCommand(string Name, string Slug) : IRequest<ErrorOr<Guid>>;

    public sealed class CreateOrganizationCommandHandler(
    DatabaseContext context,
    ICurrentUserService currentUserService
) : IRequestHandler<CreateOrganizationCommand, ErrorOr<Guid>>
    {
        public async Task<ErrorOr<Guid>> Handle(CreateOrganizationCommand request, CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(currentUserService.UserId);
            if (userId == Guid.Empty)
                return Error.Unauthorized("Missing or invalid user");

            var org = new Domain.Entities.Organization(request.Name, request.Slug, userId);

            await context.Organizations.AddAsync(org, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return org.Id;
        }
    }
}
