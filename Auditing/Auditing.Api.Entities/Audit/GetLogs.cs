using Auditing.Api.Common;
using Auditing.Api.Common.Authorization.Requirements;
using Auditing.Api.Domain.Entities;
using Auditing.Api.Infrastructure.Persistance;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Auditing.Api.Features.Logs
{
    [Authorize]
    [RoleAuthorize("Admin")]
    public class GetLogsController() : ApiControllerBase
    {
        [HttpGet("/api/logs")]
        public async Task<IResult> GetUsers([FromQuery] GetLogsWithPaginationQuery query)
        {
            var result = await Mediator.Send(query);

            return result.Match(
                id => Results.Ok(id),
                error => Results.BadRequest(error.First().Description));
        }
    }

    public record GetLogsWithPaginationQuery(int PageNumber = 1, int PageSize = 10) : IRequest<ErrorOr<List<Log>>>;

    internal sealed class GetLogsWithPaginationQueryValidator : AbstractValidator<GetLogsWithPaginationQuery>
    {
        public GetLogsWithPaginationQueryValidator()
        {
            RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1)
                .WithMessage(GetLogsWithPaginationQueryConstants.PAGENUMBER_GREATER_THAN_STRING);

            RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1)
                .WithMessage(GetLogsWithPaginationQueryConstants.PAGESIZE_GREATER_THAN_STRING);
        }
    }

    internal static class GetLogsWithPaginationQueryConstants
    {
        internal static string PAGENUMBER_GREATER_THAN_STRING = "PageNumber at least greater than or equal to 1.";
        internal static string PAGESIZE_GREATER_THAN_STRING = "PageSize at least greater than or equal to 1.";
    }

    public sealed class GetLogsItemsWithPaginationQueryHandler(DatabaseContext context) : IRequestHandler<GetLogsWithPaginationQuery, ErrorOr<List<Log>>>
    {
        private readonly DatabaseContext _context = context;

        public async Task<ErrorOr<List<Log>>> Handle(GetLogsWithPaginationQuery request, CancellationToken cancellationToken)
        {
            var x = await _context.Logs.ToListAsync();

            return x;
        }
    }
}
