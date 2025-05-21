using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Organization.Api.Common.Constants;
using Organization.Api.Domain.Mappers;

namespace Organization.Api.Domain
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddValidation(this IServiceCollection services)
        {
            return services;
        }

        public static IServiceCollection AddMapping(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(MappingProfile));

            return services;
        }
    }

}
