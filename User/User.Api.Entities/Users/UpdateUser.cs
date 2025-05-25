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
using Microsoft.AspNetCore.Authorization;
using User.API.Common.Interfaces;

namespace User.Api.Features.Users
{
    [Authorize]
    [RoleAuthorize("User")]
    public class UpdateUserController() : ApiControllerBase
    {
        [HttpPost("/api/users/me")]
        public async Task<IResult> GetUsers(
            [FromForm] string? username,
            [FromForm] string? email
            )
        {
            var result = await Mediator.Send(new UpdateUserCommand(username, email));

            return result.Match(
                id => Results.Ok(id),
                error => Results.BadRequest(error.First().Description));
        }
    }

    public record UpdateUserCommand(string? username = null, string? email = null) : IRequest<ErrorOr<Unit>>;

    internal sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {
        private readonly UserDatabaseContext _context;

        public UpdateUserCommandValidator(UserDatabaseContext context)
        {
            _context = context;

            RuleFor(user => user.username)
                .MustAsync(BeUniqueUsername!).WithMessage(UpdateUserCommandValidatorConstants.USERNAME_NOT_UNIQUE_STRING);

            RuleFor(user => user.email)
                .MustAsync(BeUniqueEmail!).WithMessage(UpdateUserCommandValidatorConstants.EMAIL_NOT_UNIQUE_STRING);

            RuleFor(user => user.username)
                .NotEmpty().WithMessage(UpdateUserCommandValidatorConstants.USERNAME_REQUIRED_STRING)
                .Length(UpdateUserCommandValidatorConstants.USERNAME_MINIMUM_LENGTH, UpdateUserCommandValidatorConstants.USERNAME_MAXIMUM_LENGTH)
                .WithMessage(UpdateUserCommandValidatorConstants.USERNAME_INVALID_LENGTH_STRING);

            RuleFor(user => user.email)
                .EmailAddress(FluentValidation.Validators.EmailValidationMode.AspNetCoreCompatible)
                .WithMessage(UpdateUserCommandValidatorConstants.EMAIL_INVALID_STRING)
                .Length(UpdateUserCommandValidatorConstants.EMAIL_MINIMUM_LENGTH, UpdateUserCommandValidatorConstants.EMAIL_MAXIMUM_LENGTH)
                .WithMessage(UpdateUserCommandValidatorConstants.EMAIL_INVALID_LENGTH_STRING);
        }

        private async Task<bool> BeUniqueUsername(string username, CancellationToken token) => !(await _context.Users.AnyAsync(x => x.Name == username, token));

        private async Task<bool> BeUniqueEmail(string email, CancellationToken token) => !(await _context.Users.AnyAsync(x => x.Email == email, token));
    }

    internal static class UpdateUserCommandValidatorConstants
    {
        public static string USERNAME_REQUIRED_STRING = "A username is required";
        public static string USERNAME_INVALID_LENGTH_STRING = $"A username needs to be between {USERNAME_MINIMUM_LENGTH} and {USERNAME_MAXIMUM_LENGTH} characters.";

        public static int USERNAME_MINIMUM_LENGTH = 4;
        public static int USERNAME_MAXIMUM_LENGTH = 50;

        public static string EMAIL_INVALID_STRING = "Supplied email is invalid";
        public static string EMAIL_INVALID_LENGTH_STRING = $"An email needs to be between {EMAIL_MINIMUM_LENGTH} and {EMAIL_MAXIMUM_LENGTH} characters.";


        public static int EMAIL_MINIMUM_LENGTH = 4;
        public static int EMAIL_MAXIMUM_LENGTH = 100;

        public static string PASSWORD_EMPTY_STRING = "A password is required";
        public static string PASSWORD_SHORT_STRING = "Your password length must be at least 12.";
        public static string PASSWORD_CONTAINS_CAPITAL_STRING = "Your password must contain at least one uppercase letter.";
        public static string PASSWORD_CONTAINS_LOWER_STRING = "Your password must contain at least one lowercase letter.";

        public static string USERNAME_NOT_UNIQUE_STRING = "Your username needs to be unique.";
        public static string EMAIL_NOT_UNIQUE_STRING = "Your email needs to be unique.";
    }

    public sealed class UpdateUserCommandHandler(UserDatabaseContext context, ICurrentUserService userService) : IRequestHandler<UpdateUserCommand, ErrorOr<Unit>>
    {
        private readonly UserDatabaseContext _context = context;


        public async Task<ErrorOr<Unit>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (userService.UserId == null || userService.UserId == Guid.Empty)
                    return Error.Validation("id", "id is required");
                if (string.IsNullOrEmpty(request.username) && string.IsNullOrEmpty(request.email))
                    return Error.Validation("Either or both the email and username are required.");

                var user = await _context.Users.SingleAsync(x => x.Id == userService.UserId, cancellationToken);

                if (user == null)
                    return Error.NotFound("User not found");

                if (!string.IsNullOrEmpty(request.username))
                    user.Name = request.username;

                if (!string.IsNullOrEmpty(request.email))
                    user.Email = request.email;

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
