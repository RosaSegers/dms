using Auditing.Api.Common.Behaviour;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Auditing.Api.Features
{
    internal static class Roles
    {
        internal static List<string> Items = new()
        {
            "Admin",
            "User"
        };
    }

    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration config)
        {
            //services.AddSingleton<IAuthorizationHandler, RoleHandler>();

            //services.AddAuthorization(options =>
            //{
            //    foreach (var permission in Roles.Items)
            //        options.AddPolicy(permission, policy =>
            //            policy.Requirements.Add(new RoleRequirement(permission)));
            //});

            services.AddMediatR(options =>
            {
                options.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
                options.AddOpenBehavior(typeof(ValidationBehaviour<,>));
                options.AddOpenBehavior(typeof(LoggingBehaviour<,>));
            });

            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

            return services;
        }
    }
}
