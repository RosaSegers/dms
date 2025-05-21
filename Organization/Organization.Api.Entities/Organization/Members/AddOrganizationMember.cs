using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Organization.Api.Common;
using Organization.Api.Common.Interfaces;
using Organization.Api.Infrastructure.Persistance;

namespace Organization.Api.Features.Organization.Members
{
    public record AddOrganizationMemberCommand(Guid UserId, Guid OrganizationId) : IRequest<ErrorOr<Unit>>;

    public sealed class AddOrganizationMemberCommandHandler(
        DatabaseContext context,
        ICurrentUserService currentUserService
    ) : IRequestHandler<AddOrganizationMemberCommand, ErrorOr<Unit>>
    {
        public async Task<ErrorOr<Unit>> Handle(AddOrganizationMemberCommand request, CancellationToken cancellationToken)
        {
            if (await context.Organizations.AllAsync(o => o.Id != request.OrganizationId))
                return Error.NotFound("Organization does not exist");

            await context.Members.AddAsync(new Domain.Entities.Member(request.OrganizationId, request.UserId), cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
