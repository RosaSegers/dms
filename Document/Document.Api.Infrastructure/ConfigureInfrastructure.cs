using Document.Api.Common.Interfaces;
using Document.Api.Infrastructure.Persistance;
using Document.Api.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Document.Api.Infrastructure.NewFolder
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IHashingService, HashingService>();

            services.AddDbContext<UserDatabaseContext>(options =>
            {
#if DEBUG
                options.UseSqlServer("server=ROSAS_LAPTOP\\SQLEXPRESS;database=Users;trusted_connection=true;TrustServerCertificate=True;");
#elif TEST
                options.UseInMemoryDatabase("Users");
#endif
            });

            using (var scope = services.BuildServiceProvider())
            {
                var dataContext = scope.GetRequiredService<UserDatabaseContext>();
                var hashingService = scope.GetRequiredService<IHashingService>();
                UserDatabaseContextSeed.SeedSampleData(dataContext, hashingService);
            }

            return services;
        }
    }
}
