using Microsoft.EntityFrameworkCore;
using User.Api.Common.Interfaces;
using User.Api.Domain.Entities;

namespace User.Api.Infrastructure.Persistance
{
    public static class UserDatabaseContextSeed
    {
        public static void SeedSampleData(UserDatabaseContext context, IHashingService hashing)
        {
            if (!context.Users.Any())
            {

                var adminUser = new Domain.Entities.User("Admin", "Admin@email.com", hashing.Hash("ACoolPassword"));
                context.Users.Add(adminUser);

                var userUser = new Domain.Entities.User("User", "User@email.com", hashing.Hash("ACoolPassword"));
                context.Users.Add(userUser);

                context.SaveChanges();
            }
        }
    }
}
