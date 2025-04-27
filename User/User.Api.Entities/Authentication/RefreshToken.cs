using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using User.Api.Common.Interfaces;
using User.Api.Infrastructure.Persistance;
using User.Api.Infrastructure.Services;
using User.API.Common;

namespace User.Api.Features.Authentication
{
    public class RefreshTokenController() : ApiControllerBase
    {
        [Authorize]
        [HttpPost("/api/auth/refresh")]
        public async Task<IResult> GetUsers([FromForm] RefreshTokenQuery query)
        {
            var result = await Mediator.Send(query);

            return result.Match(
                id => Results.Ok(id),
                error => Results.BadRequest(error.First().Description));
        }
    }

    public record RefreshTokenQuery(string RefreshToken) : IRequest<ErrorOr<RefreshTokenResult>>;
    public record RefreshTokenResult(string AccessToken, string RefreshToken);

    internal sealed class RefreshTokenQueryValidator : AbstractValidator<RefreshTokenQuery>
    {
        public RefreshTokenQueryValidator()
        {

        }
    }

    internal static class RefreshTokenQueryConstants
    {

    }

    public sealed class RefreshTokenQueryHandler(UserDatabaseContext context, IJwtTokenGenerator jwt, IRefreshTokenGenerator refresh, IHashingService hashService) : IRequestHandler<RefreshTokenQuery, ErrorOr<RefreshTokenResult>>
    {
        public async Task<ErrorOr<RefreshTokenResult>> Handle(RefreshTokenQuery request, CancellationToken cancellationToken)
        {
            var refreshToken = context.RefreshTokens.SingleOrDefault(x => x.Token == request.RefreshToken);
            if (refreshToken == null)
                return Error.Unauthorized("Invalid refresh token");

            var user = context.Users.SingleOrDefault(x => x.Id == refreshToken.UserId);
            if (user == null)
                return Error.Unauthorized("Invalid refresh token");

            var newAccessToken = jwt.GenerateToken(user);
            var newRefreshToken = await refresh.GenerateAndStoreRefreshTokenAsync(user);

            refreshToken.IsUsed = true;
            await context.SaveChangesAsync();

            return new RefreshTokenResult(newAccessToken, newRefreshToken);
        }
    }
}
