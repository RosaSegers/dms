using Document.Api.Common.Interfaces;
using Document.Api.Infrastructure.Persistance;
using Document.Api.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Document.Api.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IVirusScanner, VirusScanner>();
            services.AddSingleton<DocumentStorage>();

            return services;
        }
    }
}
