using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using User.API.Common.Models;
using User.API.Common;
using MediatR;
using FluentValidation;
using User.Api.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using User.API.Common.Constants;
using User.Api.Common.Interfaces;
using ErrorOr;
using Microsoft.AspNetCore.Http;
using User.Api.Common.Authorization.Requirements;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using User.API.Common.Interfaces;

namespace User.Api.Features.Users
{
    public class ChangePasswordController() : ApiControllerBase
    {

        [HttpPut("/api/users/password")]
        public async Task<IResult> GetUsers(
            [FromForm] string password,
            [FromForm] string? oldPassword
            )
        {
            var result = await Mediator.Send(new ChangePasswordCommand(password, oldPassword));

            return result.Match(
                id => Results.Ok(id),
                error => Results.BadRequest(error.First().Description));
        }
    }

    public record ChangePasswordCommand(string Password, string? OldPassword = null) : IRequest<ErrorOr<Unit>>;

    internal sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
    {
        public ChangePasswordCommandValidator()
        {
            RuleFor(user => user.Password)
                .NotEmpty().WithMessage(ChangePasswordCommandValidatorConstants.PASSWORD_EMPTY_STRING)
                .MinimumLength(15).WithMessage(ChangePasswordCommandValidatorConstants.PASSWORD_SHORT_STRING)
                .Matches(@"[A-Z]+").WithMessage(ChangePasswordCommandValidatorConstants.PASSWORD_CONTAINS_CAPITAL_STRING)
                .Matches(@"[a-z]+").WithMessage(ChangePasswordCommandValidatorConstants.PASSWORD_CONTAINS_LOWER_STRING);
        }
    }

    internal static class ChangePasswordCommandValidatorConstants
    {
        public static string PASSWORD_EMPTY_STRING = "A password is required.";
        public static string PASSWORD_SHORT_STRING = "Your password length must be at least 12.";
        public static string PASSWORD_CONTAINS_CAPITAL_STRING = "Your password must contain at least one uppercase letter.";
        public static string PASSWORD_CONTAINS_LOWER_STRING = "Your password must contain at least one lowercase letter.";
    }

    public sealed class ChangePasswordCommandHandler(IHashingService hashingService, UserDatabaseContext context, ICurrentUserService userService) : IRequestHandler<ChangePasswordCommand, ErrorOr<Unit>>
    {
        private readonly UserDatabaseContext _context = context;


        public async Task<ErrorOr<Unit>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (userService.UserId == null)
                    return Error.Validation("id", "id is required");
                var user = await _context.Users.SingleAsync(x => x.Id == userService.UserId, cancellationToken);

                if (user == null)
                    return Error.NotFound("User not found");

                if (!string.IsNullOrEmpty(request.OldPassword))
                {
                    if(user.Password != hashingService.Hash(request.OldPassword))
                        return Error.Validation("oldPassword", "The passwords do not match.");

                    user.Password = hashingService.Hash(request.Password);

                    await _context.SaveChangesAsync(cancellationToken);

                    return Unit.Value;
                }

                user.Password = hashingService.Hash(request.Password);

                await _context.SaveChangesAsync(cancellationToken);

                return Unit.Value;
            }
            catch (Exception ex)
            {
                return Error.Unexpected(ex.Message);
            }
        }
    }
}
