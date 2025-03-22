using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using User.Api.Infrastructure.Persistance;
using User.Api.Infrastructure.Services;
using User.API.Common.Interfaces;

namespace User.Api.Infrastructure
{
    public static class DependencyInjection
    {
        public async static Task<IServiceCollection> AddInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            services.AddDbContext<UserDatabaseContext>(options =>
            {
                options.UseSqlServer("server=ROSAS_LAPTOP\\SQLEXPRESS;database=Users;trusted_connection=true;TrustServerCertificate=True;");
            });

            using (var scope = services.BuildServiceProvider())
            {
                var dataContext = scope.GetRequiredService<UserDatabaseContext>();
                await UserDatabaseContextSeed.SeedSampleData(dataContext);
            }

            return services;
        }
    }
}
