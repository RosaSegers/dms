﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using User.Api.Common.Interfaces;
using User.Api.Infrastructure.Persistance;
using User.Api.Infrastructure.Services;
using User.API.Common.Interfaces;

namespace User.Api.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IHashingService, HashingService>();

            services.AddDbContext<UserDatabaseContext>(options =>
            {
#if TEST
                options.UseInMemoryDatabase("UserDatabase");
#elif DEBUG
                options.UseSqlServer("server=ROSAS_LAPTOP\\SQLEXPRESS;database=Users;trusted_connection=true;TrustServerCertificate=True;");
#else
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
#endif

            });

            using (var scope = services.BuildServiceProvider())
            {
                var dataContext = scope.GetRequiredService<UserDatabaseContext>();
                dataContext.Database.Migrate();
                var hashingService = scope.GetRequiredService<IHashingService>();
                UserDatabaseContextSeed.SeedSampleData(dataContext, hashingService);
            }

            return services;
        }
    }
}
