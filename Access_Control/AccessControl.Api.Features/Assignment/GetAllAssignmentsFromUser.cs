using AccessControl.Api.Common;
using AccessControl.Api.Infrastructure.Persistance;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static AccessControl.Api.Features.Assignment.GetAssignmentsController;

namespace AccessControl.Api.Features.Assignment
{
    [ApiController]
    public class GetAssignmentsController() : ApiControllerBase
    {
        [HttpGet("/api/assignments")]
        public async Task<IResult> GetAssignments([FromQuery] GetAssignmentsQuery query)
        {
            var result = await Mediator.Send(query);

            return result.Match(
                ok => Results.Ok(ok),
                error => Results.BadRequest(error.First().Description));
        }

        public record GetAssignmentsQuery(Guid UserId, Guid? ResourceId = null)
            : IRequest<ErrorOr<List<Domain.Entities.Assignment>>>;
    }

    internal sealed class GetAssignmentsQueryValidator : AbstractValidator<GetAssignmentsQuery>
    {
        public GetAssignmentsQueryValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId is required.");
        }
    }

    public sealed class GetAssignmentsQueryHandler(Context context)
        : IRequestHandler<GetAssignmentsQuery, ErrorOr<List<Domain.Entities.Assignment>>>
    {
        private readonly Context _context = context;

        public async Task<ErrorOr<List<Domain.Entities.Assignment>>> Handle(GetAssignmentsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Assignment
                .Where(a => a.UserId == request.UserId);

            if (request.ResourceId is not null)
                query = query.Where(a => a.ResourceId == request.ResourceId);

            return await query.ToListAsync();
        }
    }
}