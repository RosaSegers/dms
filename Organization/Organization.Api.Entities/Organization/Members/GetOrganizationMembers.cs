using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Organization.Api.Common;
using Organization.Api.Common.Interfaces;
using Organization.Api.Common.Mappers;
using Organization.Api.Common.Models;
using Organization.Api.Domain.Entities;
using Organization.Api.Infrastructure.Persistance;

namespace Organization.Api.Features.Organization.Members
{
    public class GetOrganizationMembersController : ApiControllerBase
    {
        [HttpGet]
        public async Task<IResult> AddOrganizationMembers([FromBody] GetOrganizationMembersCommand command)
        {
            var result = await Mediator.Send(command);

            return result.Match(
                id => Results.NoContent(),
                error => Results.BadRequest(error.First().Description));
        }
    }

    public record GetOrganizationMembersCommand(Guid OrganizationId, int page = 1, int pageSize = 10) : IRequest<ErrorOr<PaginatedList<Member>>>;

    public sealed class RemoveOrganizationCommandHandler(
        DatabaseContext context,
        ICurrentUserService currentUserService
    ) : IRequestHandler<GetOrganizationMembersCommand, ErrorOr<PaginatedList<Member>>>
    {
        public async Task<ErrorOr<PaginatedList<Member>>> Handle(GetOrganizationMembersCommand request, CancellationToken cancellationToken)
        {
            return await context.Members.Where(x => x.OrganizationId == request.OrganizationId).PaginatedListAsync(request.page, request.pageSize);
        }
    }
}
