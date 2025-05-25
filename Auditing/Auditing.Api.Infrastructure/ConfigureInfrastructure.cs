using Auditing.Api.Common.Interfaces;
using Auditing.Api.Infrastructure.Persistance;
using Auditing.Api.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Auditing.Api.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMemoryCache();
            services.AddSingleton<ICacheService, CacheService>();

            services.AddScoped<ICurrentUserService, CurrentUserService>();


            services.AddDbContext<DatabaseContext>(options =>
            {
#if TEST
                options.UseInMemoryDatabase("LogDatabase");
#elif DEBUG
                options.UseSqlServer("server=ROSAS_LAPTOP\\SQLEXPRESS;database=Logs;trusted_connection=true;TrustServerCertificate=True;");
#else
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
#endif

            });

            services.AddSingleton<RabbitMqLogConsumer>();
            services.AddHostedService<RabbitMqBackgroundService>();

            using (var scope = services.BuildServiceProvider())
            {
                var dataContext = scope.GetRequiredService<DatabaseContext>();
#if !TEST
                dataContext.Database.Migrate();
#endif
            }

            return services;
        }
    }
}
