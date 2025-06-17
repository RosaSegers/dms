using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using User.API.Common.Interfaces;

namespace User.Api.Infrastructure.Persistance.Factories
{
    public class UserDatabaseContextFactory : IDesignTimeDbContextFactory<UserDatabaseContext>
    {
        public UserDatabaseContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("Secrets/user-secrets.json", optional: true)
                .AddUserSecrets<UserDatabaseContextFactory>(optional: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<UserDatabaseContext>();

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            optionsBuilder.UseSqlServer(connectionString);

            ICurrentUserService currentUserService = new DesignTimeCurrentUserService();
            return new UserDatabaseContext(currentUserService, optionsBuilder.Options);
        }
    }

    public class DesignTimeCurrentUserService : ICurrentUserService
    {
        public Guid? UserId => Guid.NewGuid();
    }
}