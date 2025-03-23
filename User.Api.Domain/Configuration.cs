using FluentValidation;
using User.API.Common.Constants;

namespace User.Api.Domain
{
    public class UserConfiguration : AbstractValidator<Entities.User>
    {

        public UserConfiguration()
        {
            RuleFor(x => x.Id)
                .NotEmpty();

            //todo Use global configs to create a more abstract layer
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(UserConstants.USERNAME_REQUIRED_STRING)
                .Length(UserConstants.USERNAME_MINIMUM_LENGTH, UserConstants.USERNAME_MAXIMUM_LENGTH)
                .WithMessage(UserConstants.USERNAME_INVALID_LENGTH_STRING);

            RuleFor(x => x.Email)
                .EmailAddress(FluentValidation.Validators.EmailValidationMode.AspNetCoreCompatible)
                .WithMessage(UserConstants.EMAIL_INVALID_STRING)
                .Length(UserConstants.EMAIL_MINIMUM_LENGTH, UserConstants.EMAIL_MAXIMUM_LENGTH)
                .WithMessage(UserConstants.EMAIL_INVALID_LENGTH_STRING);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(UserConstants.PASSWORD_EMPTY_STRING)
                .MinimumLength(12).WithMessage(UserConstants.PASSWORD_SHORT_STRING)
                .Matches(@"[A-Z]+").WithMessage(UserConstants.PASSWORD_CONTAINS_CAPITAL_STRING)
                .Matches(@"[a-z]+").WithMessage(UserConstants.PASSWORD_CONTAINS_LOWER_STRING);
        }
    }
}
