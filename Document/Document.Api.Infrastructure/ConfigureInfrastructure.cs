using Document.Api.Common.Interfaces;
using Document.Api.Infrastructure.Persistance;
using Document.Api.Infrastructure.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Document.Api.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton(s =>
            {
                var config = s.GetRequiredService<IConfiguration>();
                var endpoint = config["CosmosDb:Endpoint"];
                var key = config["CosmosDb:Key"];
                return new CosmosClient(endpoint, key);
            });


            services.AddMemoryCache();
            services.AddSingleton<ICacheService, CacheService>();

            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IVirusScanner, VirusScanner>();
            services.AddSingleton<IDocumentStorage, DocumentStorage>();

            return services;
        }
    }
}
