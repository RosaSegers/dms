using Microsoft.EntityFrameworkCore;
using User.API.Common.Interfaces;

namespace User.Api.Infrastructure.Persistance
{
    public class UserDatabaseContext(ICurrentUserService userService, DbContextOptions options) : ShadowContext(userService, options)
    {
        public virtual DbSet<Domain.Entities.User> Users { get; set; }
    }
}
