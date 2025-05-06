using Microsoft.EntityFrameworkCore;
using Organization.Api.Common.Interfaces;

namespace Organization.Api.Infrastructure.Persistance
{
    public class DatabaseContext(ICurrentUserService userService, DbContextOptions options) : ShadowContext(userService, options)
    {
        public virtual DbSet<Domain.Entities.Organization> Organizations { get; set; }
        public virtual DbSet<Domain.Entities.Member> Members { get; set; }
        public virtual DbSet<Domain.Entities.Invite> Invites { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

    }
}
