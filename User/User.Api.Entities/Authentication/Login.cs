using Azure.Core;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using User.Api.Common.Interfaces;
using User.Api.Infrastructure.Persistance;
using User.Api.Infrastructure.Services;
using User.API.Common;

namespace User.Api.Features.Authentication
{
    //[AllowAnonymous]
    public class LoginController() : ApiControllerBase
    {
        [HttpPost("/api/auth/login")]
        public async Task<IResult> GetUsers([FromForm] LoginQuery query)
        {
            var result = await Mediator.Send(query);

            return result.Match(
                id => Results.Ok(id),
                error => Results.BadRequest(error.First().Code));
        }
    }

    public record LoginQuery(string Email, string Password) : IRequest<ErrorOr<LoginResult>>;
    public record LoginResult(string AccessToken, string RefreshToken);

    internal sealed class LoginQueryValidator : AbstractValidator<LoginQuery>
    {
        public LoginQueryValidator()
        {

        }
    }

    internal static class LoginQueryConstants
    {

    }

    public sealed class LoginQueryHandler(UserDatabaseContext context, IJwtTokenGenerator jwt, IRefreshTokenGenerator refresh, IHashingService hashService) : IRequestHandler<LoginQuery, ErrorOr<LoginResult>>
    {
        public async Task<ErrorOr<LoginResult>> Handle(LoginQuery request, CancellationToken cancellationToken)
        {
            var normalisedEmail = request.Email.ToLowerInvariant();
            var user = await context.Users.SingleAsync(u => u.Email == normalisedEmail);

            if (user is null)
                return Error.Unauthorized("Invalid credentials.");

            if (user.LoginAttempts >= 3 && user.LastFailedLoginAttempt?.AddMinutes(5) > DateTime.UtcNow)
                return Error.Unauthorized("Too many failed login attemtps.");

            if (!hashService.Validate(request.Password, user.Password))
            {
                user.LastFailedLoginAttempt = DateTime.UtcNow;
                user.LoginAttempts++;
                await context.SaveChangesAsync(cancellationToken);
                return Error.Unauthorized("Invalid credentials.");
            }

            user.LoginAttempts = 0;
            user.LastFailedLoginAttempt = null;
            await context.SaveChangesAsync(cancellationToken);

            var accessTokenTask = Task.Run(() => jwt.GenerateToken(user));
            var refreshTokenTask = refresh.GenerateAndStoreRefreshTokenAsync(user);

            await Task.WhenAll(accessTokenTask, refreshTokenTask);

            return new LoginResult(accessTokenTask.Result, refreshTokenTask.Result);
        }
    }
}

