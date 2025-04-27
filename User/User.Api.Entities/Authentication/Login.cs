using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using User.Api.Common.Interfaces;
using User.Api.Infrastructure.Persistance;
using User.Api.Infrastructure.Services;
using User.API.Common;

namespace User.Api.Features.Authentication
{
    public class LoginController() : ApiControllerBase
    {
        [HttpPost("/api/auth/login")]
        public async Task<IResult> GetUsers([FromForm] LoginQuery query)
        {
            var result = await Mediator.Send(query);

            return result.Match(
                id => Results.Ok(id),
                error => Results.BadRequest(error.First().Description));
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
            var user = context.Users.Where(u => u.Email == request.Email)
                .Include(u => u.Roles)
                .ThenInclude(r => r.Permissions)
                .SingleOrDefault();

            if (user is null)
                return Error.Unauthorized("Invalid credentials.");
            if (!hashService.Validate(request.Password, user.Password))
                return Error.Unauthorized("Invalid credentials.");


            var accessToken = jwt.GenerateToken(user);
            var refreshToken = await refresh.GenerateAndStoreRefreshTokenAsync(user);

            return new LoginResult(accessToken, refreshToken);
        }
    }
}
