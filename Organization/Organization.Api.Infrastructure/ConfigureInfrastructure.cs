using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Organization.Api.Common.Interfaces;
using Organization.Api.Infrastructure.Persistance;
using Organization.Api.Infrastructure.Services;

namespace Organization.Api.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IHashingService, HashingService>();

            services.AddDbContext<DatabaseContext>(options =>
            {
#if DEBUG
                options.UseSqlServer("server=ROSAS_LAPTOP\\SQLEXPRESS;database=Users;trusted_connection=true;TrustServerCertificate=True;");
#elif TEST
                options.UseInMemoryDatabase("Users");
#endif
            });

            using (var scope = services.BuildServiceProvider())
            {
                var dataContext = scope.GetRequiredService<DatabaseContext>();
                var hashingService = scope.GetRequiredService<IHashingService>();
                DatabaseContextSeed.SeedSampleData(dataContext, hashingService);
            }

            return services;
        }
    }
}
