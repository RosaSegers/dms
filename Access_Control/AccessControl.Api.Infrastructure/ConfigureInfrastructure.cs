using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AccessControl.Api.Common.Interfaces;
using AccessControl.Api.Infrastructure.Persistance;
using AccessControl.Api.Infrastructure.Services;

namespace AccessControl.Api.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IHashingService, HashingService>();

            services.AddDbContext<Context>(options =>
            {
#if DEBUG
                options.UseSqlServer("server=ROSAS_LAPTOP\\SQLEXPRESS;database=Users;trusted_connection=true;TrustServerCertificate=True;");
#elif TEST
                options.UseInMemoryDatabase("Users");
#endif
            });

            using (var scope = services.BuildServiceProvider())
            {
                var dataContext = scope.GetRequiredService<Context>();
                var hashingService = scope.GetRequiredService<IHashingService>();
                UserDatabaseContextSeed.SeedSampleData(dataContext, hashingService);
            }

            return services;
        }
    }
}
