using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using User.Api.Infrastructure.Persistance;
using User.API.Common;
using User.API.Common.Enums;
using User.API.Common.Mappers;
using User.API.Common.Models;

namespace User.Api.Features.Users
{

    public class GetUsersController() : ApiControllerBase
    {
        [HttpGet("/api/users")]
        public async Task<PaginatedList<Domain.Entities.User>> GetUsers([FromQuery] GetUsersWithPaginationQuery query)
        {
            var result = await Mediator.Send(query);

            return result;
        }
    }

    public record UserResponse(PaginatedList<Domain.Entities.User> users, StatusCodes StatusCode);
    public record GetUsersWithPaginationQuery(int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedList<Domain.Entities.User>>;

    internal sealed class GetUsersWithPaginationQueryValidator : AbstractValidator<GetUsersWithPaginationQuery>
    {
        public GetUsersWithPaginationQueryValidator()
        {
            RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1)
                .WithMessage(GetUsersWithPaginationQueryConstants.PAGENUMBER_GREATER_THAN_STRING);

            RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1)
                .WithMessage(GetUsersWithPaginationQueryConstants.PAGESIZE_GREATER_THAN_STRING);
        }
    }

    internal static class GetUsersWithPaginationQueryConstants
    {
        internal static string PAGENUMBER_GREATER_THAN_STRING = "PageNumber at least greater than or equal to 1.";
        internal static string PAGESIZE_GREATER_THAN_STRING = "PageSize at least greater than or equal to 1.";
    }

    public sealed class GetUserItemsWithPaginationQueryHandler(UserDatabaseContext context) : IRequestHandler<GetUsersWithPaginationQuery, PaginatedList<Domain.Entities.User>>
    {
        private readonly UserDatabaseContext _context = context;


        public async Task<PaginatedList<Domain.Entities.User>> Handle(GetUsersWithPaginationQuery request, CancellationToken cancellationToken)
        {
            var x = await _context.Users
                .OrderBy(item => item.Name)
                .PaginatedListAsync(request.PageNumber, request.PageSize);

            return x;
        }
    }


}
