using AccessControl.Api.Common.Authorization.Requirements;
using AccessControl.Api.Common.Behaviour;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AccessControl.Api.Features
{
    internal static class Role
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
            services.AddSingleton<IAuthorizationHandler, RoleHandler>();

            services.AddAuthorization(options =>
            {
                foreach (var permission in Role.Items)
                    options.AddPolicy(permission, policy =>
                        policy.Requirements.Add(new RoleRequirement(permission)));
            });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(config["Jwt:Key"] ?? throw new Exception()))
                    };
                });


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
