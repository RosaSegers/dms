using Microsoft.EntityFrameworkCore;
using User.Api.Common.Interfaces;

namespace User.Api.Infrastructure.Persistance
{
    public static class UserDatabaseContextSeed
    {
        public static void SeedSampleData(UserDatabaseContext context, IHashingService hashing)
        {
            if (!context.Users.Any())
            {
                var entity = new Domain.Entities.User("Rosa", "email@email.com", hashing.Hash("ACoolPassword"));

                context.Users.Add(entity);

                context.SaveChanges();
            }
        }
    }
}
