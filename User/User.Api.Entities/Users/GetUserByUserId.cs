using AutoMapper;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using User.Api.Common.Authorization.Requirements;
using User.Api.Infrastructure.Persistance;
using User.API.Common;
using User.API.Common.Interfaces;

namespace User.Api.Features.Users
{
    [Authorize]
    [RoleAuthorize("User")]
    public class GetUserByUserIdController() : ApiControllerBase
    {
        [HttpGet("/api/users/me")]
        public async Task<IResult> GetUserByUserId([FromQuery] GetUserByUserIdQuery query)
        {
            var result = await Mediator.Send(query);

            return result.Match(
                id => Results.Ok(id),
                error => Results.BadRequest(error.First().Description));
        }
    }

    public record GetUserByUserIdQuery() : IRequest<ErrorOr<Domain.Dtos.User>>;

    public sealed class GetUserByUserIdQueryHandler(UserDatabaseContext context, IMapper _mapper, ICurrentUserService userService) : IRequestHandler<GetUserByUserIdQuery, ErrorOr<Domain.Dtos.User>>
    {
        private readonly UserDatabaseContext _context = context;

        public async Task<ErrorOr<Domain.Dtos.User>> Handle(GetUserByUserIdQuery request, CancellationToken cancellationToken)
        {
            var x = await _context.Users.SingleAsync(x => x.Id == userService.UserId, cancellationToken: cancellationToken);
            return _mapper.Map<Domain.Dtos.User>(x) ?? new();
        }
    }
}
