using Microsoft.EntityFrameworkCore;
using User.API.Common.Interfaces;

namespace User.Api.Infrastructure.Persistance
{
    public class UserDatabaseContext : ShadowContext
    {
        public DbSet<Domain.Entities.User> Users { get; set; }

        public UserDatabaseContext(ICurrentUserService userService, DbContextOptions options) : base(userService, options)
        {
        }

    }
}
