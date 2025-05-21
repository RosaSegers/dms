using AccessControl.Api.Domain.Mappers;
using Microsoft.Extensions.DependencyInjection;

namespace AccessControl.Api.Domain
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
