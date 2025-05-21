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

namespace User.Api.Features.Users
{
    public class CreateUsersController() : ApiControllerBase
    {

        [HttpPost("/api/users")]
        public async Task<IResult> GetUsers([FromForm] CreateUserQuery query)
        {
            var result = await Mediator.Send(query);

            return result.Match(
                id => Results.Ok(id),
                error => Results.BadRequest(error.First().Description));
        }
    }

    public record CreateUserQuery(string username, string email, string password) : IRequest<ErrorOr<Guid>>;

    internal sealed class CreateUserQueryValidator : AbstractValidator<CreateUserQuery>
    {
        private readonly UserDatabaseContext _context;

        public CreateUserQueryValidator(UserDatabaseContext context)
        {
            _context = context;

            RuleFor(user => user.username)
                .MustAsync(BeUniqueUsername).WithMessage(CreateUserQueryValidatorConstants.USERNAME_NOT_UNIQUE_STRING);

            RuleFor(user => user.email)
                .MustAsync(BeUniqueEmail).WithMessage(CreateUserQueryValidatorConstants.EMAIL_NOT_UNIQUE_STRING);

            RuleFor(user => user.username)
                .NotEmpty().WithMessage(CreateUserQueryValidatorConstants.USERNAME_REQUIRED_STRING)
                .Length(CreateUserQueryValidatorConstants.USERNAME_MINIMUM_LENGTH, CreateUserQueryValidatorConstants.USERNAME_MAXIMUM_LENGTH)
                .WithMessage(CreateUserQueryValidatorConstants.USERNAME_INVALID_LENGTH_STRING);

            RuleFor(user => user.email)
                .EmailAddress(FluentValidation.Validators.EmailValidationMode.AspNetCoreCompatible)
                .WithMessage(CreateUserQueryValidatorConstants.EMAIL_INVALID_STRING)
                .Length(CreateUserQueryValidatorConstants.EMAIL_MINIMUM_LENGTH, CreateUserQueryValidatorConstants.EMAIL_MAXIMUM_LENGTH)
                .WithMessage(CreateUserQueryValidatorConstants.EMAIL_INVALID_LENGTH_STRING);

            RuleFor(user => user.password)
                .NotEmpty().WithMessage(CreateUserQueryValidatorConstants.PASSWORD_EMPTY_STRING)
                .MinimumLength(15).WithMessage(CreateUserQueryValidatorConstants.PASSWORD_SHORT_STRING)
                .Matches(@"[A-Z]+").WithMessage(CreateUserQueryValidatorConstants.PASSWORD_CONTAINS_CAPITAL_STRING)
                .Matches(@"[a-z]+").WithMessage(CreateUserQueryValidatorConstants.PASSWORD_CONTAINS_LOWER_STRING);
        }

        private async Task<bool> BeUniqueUsername(string username, CancellationToken token) => !(await _context.Users.AnyAsync(x => x.Name == username, token));

        private async Task<bool> BeUniqueEmail(string email, CancellationToken token) => !(await _context.Users.AnyAsync(x => x.Email == email, token));
    }

    internal static class CreateUserQueryValidatorConstants
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

    public sealed class CreateUserQueryHandler(IHashingService hashingService, UserDatabaseContext context) : IRequestHandler<CreateUserQuery, ErrorOr<Guid>>
    {
        private readonly UserDatabaseContext _context = context;


        public async Task<ErrorOr<Guid>> Handle(CreateUserQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var user = new Domain.Entities.User(request.username, request.email, hashingService.Hash(request.password));
                await _context.Users.AddAsync(user, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                return user.Id;
            }
            catch (Exception ex)
            {
                return Error.Unexpected(ex.Message);
            }
        }
    }
}
