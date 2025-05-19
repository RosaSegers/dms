using Document.Api.Common.Interfaces;
using Document.Api.Infrastructure.Persistance;
using Document.Api.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Document.Api.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddSingleton<ICacheService, CacheService>();

            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IVirusScanner, VirusScanner>();
            services.AddSingleton<IDocumentStorage, DocumentStorage>();

            return services;
        }
    }
}
