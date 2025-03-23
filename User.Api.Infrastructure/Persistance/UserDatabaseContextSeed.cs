using Microsoft.EntityFrameworkCore;

namespace User.Api.Infrastructure.Persistance
{
    public static class UserDatabaseContextSeed
    {
        public static void SeedSampleData(UserDatabaseContext context)
        {
            if (!context.Users.Any())
            {
                var entity = new Domain.Entities.User("Rosa", "email@email.com", "ACoolPassword");

                context.Users.Add(entity);

                context.SaveChanges();
            }
        }
    }
}
