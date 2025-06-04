using System.Text.Json;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using User.Api.Common.Interfaces;
using User.Api.Infrastructure.Persistance;
using User.API.Common;

namespace User.Api.Features.Users
{
    [AllowAnonymous]
    public class CreateUserController() : ApiControllerBase
    {

        [HttpPost("/api/users")]
        public async Task<IResult> GetUsers([FromForm] CreateUserCommand Command)
        {
            var result = await Mediator.Send(Command);

            return result.Match(
                id => Results.Ok(id),
                error => Results.BadRequest(error.First().Code));
        }
    }

    public record CreateUserCommand(string Username, string Email, string Password) : IRequest<ErrorOr<Guid>>;

    internal sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        private readonly UserDatabaseContext _context;

        public CreateUserCommandValidator(UserDatabaseContext context)
        {
            _context = context;

            RuleFor(user => user.Username)
                .MustAsync(BeUniqueUsername).WithMessage(CreateUserCommandValidatorConstants.USERNAME_NOT_UNIQUE_STRING);

            RuleFor(user => user.Email)
                .MustAsync(BeUniqueEmail).WithMessage(CreateUserCommandValidatorConstants.EMAIL_NOT_UNIQUE_STRING);

            RuleFor(user => user.Username)
                .NotEmpty().WithMessage(CreateUserCommandValidatorConstants.USERNAME_REQUIRED_STRING)
                .Length(CreateUserCommandValidatorConstants.USERNAME_MINIMUM_LENGTH, CreateUserCommandValidatorConstants.USERNAME_MAXIMUM_LENGTH)
                .WithMessage(CreateUserCommandValidatorConstants.USERNAME_INVALID_LENGTH_STRING);

            RuleFor(user => user.Email)
                .EmailAddress(FluentValidation.Validators.EmailValidationMode.AspNetCoreCompatible)
                .WithMessage(CreateUserCommandValidatorConstants.EMAIL_INVALID_STRING)
                .Length(CreateUserCommandValidatorConstants.EMAIL_MINIMUM_LENGTH, CreateUserCommandValidatorConstants.EMAIL_MAXIMUM_LENGTH)
                .WithMessage(CreateUserCommandValidatorConstants.EMAIL_INVALID_LENGTH_STRING);

            RuleFor(user => user.Password)
                .NotEmpty().WithMessage(CreateUserCommandValidatorConstants.PASSWORD_EMPTY_STRING)
                .MinimumLength(15).WithMessage(CreateUserCommandValidatorConstants.PASSWORD_SHORT_STRING)
                .Matches(@"[A-Z]+").WithMessage(CreateUserCommandValidatorConstants.PASSWORD_CONTAINS_CAPITAL_STRING)
                .Matches(@"[a-z]+").WithMessage(CreateUserCommandValidatorConstants.PASSWORD_CONTAINS_LOWER_STRING);
        }

        private async Task<bool> BeUniqueUsername(string username, CancellationToken token) => !(await _context.Users.AnyAsync(x => x.Name == username, token));

        private async Task<bool> BeUniqueEmail(string email, CancellationToken token) => !(await _context.Users.AnyAsync(x => x.Email == email, token));
    }

    internal static class CreateUserCommandValidatorConstants
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

    public sealed class CreateUserCommandHandler(IHashingService hashingService, UserDatabaseContext context) : IRequestHandler<CreateUserCommand, ErrorOr<Guid>>
    {
        private readonly UserDatabaseContext _context = context;


        public async Task<ErrorOr<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = new Domain.Entities.User(
                    request.Username,
                    request.Email,
                    hashingService.Hash(request.Password)
                );

                await _context.Users.AddAsync(user, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                return user.Id;
            }
            catch (DbUpdateException)
            {
                return Error.Validation("Database", "A user with this email or username already exists.");
            }
            catch (Exception ex)
            {
                return Error.Unexpected(ex.Message);
            }
        }

    }
}
