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
                var create = new Permission("User.CREATE", "Allows creating of users");
                var read = new Permission("User.READ", "Allows reading of users");
                var update = new Permission("User.UPDATE", "Allows updating of users");
                var delete = new Permission("User.DELETE", "Allows deleting of users");


                var admin = new Role("Admin", new List<Permission>
                {
                    create, read, update, delete
                });

                var adminUser = new Domain.Entities.User("Admin", "Admin@email.com", hashing.Hash("ACoolPassword"));
                adminUser.AddRole(admin);
                context.Users.Add(adminUser);

                context.SaveChanges();

                var user = new Role("User", new List<Permission>
                {
                    read
                });

                var userUser = new Domain.Entities.User("User", "User@email.com", hashing.Hash("ACoolPassword"));
                userUser.AddRole(user);
                context.Users.Add(userUser);

                context.SaveChanges();
            }
        }
    }
}
