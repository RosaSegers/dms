using System.Text;
using Document.Api.Common.Authorization.Requirements;
using Document.Api.Common.Behaviour;
using Document.Api.Infrastructure.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace Document.Api.Features
{
    internal static class Roles
    {
        internal static List<string> Items = new()
        {
            "Admin",
            "User",
            "Guest"
        };
    }

    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration config)
        {
            services.AddScoped<RabbitMqLogProducer>();

            services.AddAuthorization(options =>
            {
                foreach (var permission in Roles.Items)
                {
                    options.AddPolicy(permission, policy =>
                        policy.Requirements.Add(new RoleRequirement(permission)));
                }
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
                            Encoding.UTF8.GetBytes(config["Jwt:Key"] ?? throw new Exception("JWT key not found")))
                    };
                });

            var env = services.BuildServiceProvider().GetRequiredService<IHostEnvironment>();
            services.AddMediatR(options =>
            {
                options.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);

                // Add behaviors only in non-Test environments
                if (!env.IsEnvironment("Test"))
                {
                    options.AddOpenBehavior(typeof(ValidationBehaviour<,>));
                    options.AddOpenBehavior(typeof(LoggingBehaviour<,>));
                }
            });

            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

            return services;
        }

    }
}
