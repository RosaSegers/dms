//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Design;
//using Microsoft.Extensions.Configuration;
//using User.API.Common.Interfaces;

//namespace User.Api.Infrastructure.Persistance.Factories
//{
//    public class UserDatabaseContextFactory() : IDesignTimeDbContextFactory<UserDatabaseContext>
//    {
//        public UserDatabaseContext CreateDbContext(string[] args)
//        {
//            var configuration = new ConfigurationBuilder()
//                .SetBasePath(Directory.GetCurrentDirectory())
//                .AddJsonFile("/Secrets/user-secrets.json", optional: false)
//                .Build();

//            var optionsBuilder = new DbContextOptionsBuilder<UserDatabaseContext>();

//            var connectionString = configuration.GetConnectionString("DefaultConnection");

//            optionsBuilder.UseSqlServer(connectionString);

//            // Dummy service implementation to satisfy constructor
//            ICurrentUserService currentUserService = new DesignTimeCurrentUserService();

//            return new UserDatabaseContext(currentUserService, optionsBuilder.Options);
//        }
//    }

//    public class DesignTimeCurrentUserService : ICurrentUserService
//    {
//        public Guid? UserId => Guid.NewGuid();
//    }
//}