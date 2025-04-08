using Document.Api.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Document.Api.Infrastructure.Persistance
{
    public class UserDatabaseContext(ICurrentUserService userService, DbContextOptions options) : ShadowContext(userService, options)
    {
        //public DbSet<Domain.Entities.User> Users { get; set; }
    }
}
