using Auditing.Api.Common.Interfaces;
using Auditing.Api.Infrastructure.Persistance;
using Auditing.Api.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Auditing.Api.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddSingleton<ICacheService, CacheService>();

            services.AddScoped<ICurrentUserService, CurrentUserService>();

            var logConsumer = new RabbitMqLogConsumer();
            logConsumer.StartListening();

            return services;
        }
    }
}
