using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Organization.Api.Common.Constants;
using Organization.Api.Domain.Mappers;

namespace Organization.Api.Domain
{
    internal sealed class UserValidator : AbstractValidator<Entities.User>
    {

        public UserValidator()
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

    public static class DependencyInjection
    {
        public static IServiceCollection AddValidation(this IServiceCollection services)
        {
            services.AddValidatorsFromAssemblyContaining<UserValidator>();

            return services;
        }

        public static IServiceCollection AddMapping(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(MappingProfile));

            return services;
        }
    }

}
