using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using User.Api.Infrastructure.Persistance;
using User.API.Common.Interfaces;

namespace User.Api.Infrastructure.Persistance.Factories
{
    public class UserDatabaseContextFactory : IDesignTimeDbContextFactory<UserDatabaseContext>
    {
        public UserDatabaseContext CreateDbContext(string[] args)
        {
            // Build config to read connection string, adjust path if needed
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // might need adjustment depending on where you run this
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<UserDatabaseContext>();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            optionsBuilder.UseSqlServer(connectionString);

            // Dummy service implementation to satisfy constructor
            ICurrentUserService currentUserService = new DesignTimeCurrentUserService();

            return new UserDatabaseContext(currentUserService, optionsBuilder.Options);
        }
    }

    public class DesignTimeCurrentUserService : ICurrentUserService
    {
        public Guid? UserId => Guid.NewGuid();
    }
}