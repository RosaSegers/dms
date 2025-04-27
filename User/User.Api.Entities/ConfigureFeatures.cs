using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Runtime.CompilerServices;
using System.Text;
using User.Api.Common.Behaviour;
using User.Api.Domain;
using User.Api.Infrastructure.Services;

namespace User.Api.Features
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration config)
        {
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options => {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(config["Jwt:Key"]))
                    };
                });

            services.AddAuthorization();

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
