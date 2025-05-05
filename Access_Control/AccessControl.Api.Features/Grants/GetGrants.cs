using AccessControl.Api.Common;
using AccessControl.Api.Domain.Entities;
using AccessControl.Api.Infrastructure.Persistance;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AccessControl.Api.Features.Grants
{
    [ApiController]
    public class GetGrantsController() : ApiControllerBase
    {
        [HttpGet("/api/grants")]
        public async Task<IResult> GetGrants([FromQuery] GetGrantsQuery query)
        {
            var result = await Mediator.Send(query);

            return result.Match(
                ok => Results.Ok(ok),
                error => Results.BadRequest(error.First().Description));
        }

        public record GetGrantsQuery(Guid UserId, Guid? ResourceId = null) : IRequest<ErrorOr<List<Grant>>>;

        internal sealed class GetGrantsValidator : AbstractValidator<GetGrantsQuery>
        {
            public GetGrantsValidator()
            {
                RuleFor(x => x.UserId).NotEmpty();
            }
        }

        public sealed class GetGrantsHandler(Context context)
            : IRequestHandler<GetGrantsQuery, ErrorOr<List<Grant>>>
        {
            public async Task<ErrorOr<List<Grant>>> Handle(GetGrantsQuery request, CancellationToken cancellationToken)
            {
                var query = context.Grants
                    .Where(x => x.UserId == request.UserId);

                if (request.ResourceId.HasValue)
                    query = query.Where(x => x.ResourceId == request.ResourceId.Value);

                return await query.ToListAsync();
            }
        }
    }
}