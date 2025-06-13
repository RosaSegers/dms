using Document.Api.Common.Interfaces;
using Document.Api.Infrastructure.Background.Interfaces;
using Document.Api.Infrastructure.Background;
using Document.Api.Infrastructure.Persistance;
using Document.Api.Infrastructure.Services;
using Document.Api.Infrastructure.Services.Background;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Document.Api.Infrastructure.Services.Interface;

namespace Document.Api.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<IDocumentScanQueue, InMemoryDocumentScanQueue>();
            services.AddSingleton<IDocumentStatusService, InMemoryDocumentStatusService>();

            services.AddHostedService<VirusScanBackgroundService>();

            services.AddMemoryCache();
            services.AddSingleton<ICacheService, CacheService>();

            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IVirusScanner, VirusScanner>();
            services.AddSingleton<IDocumentStorage, DocumentStorage>();

            return services;
        }
    }
}
