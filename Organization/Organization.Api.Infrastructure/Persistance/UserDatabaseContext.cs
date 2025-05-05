using Microsoft.EntityFrameworkCore;
using Organization.Api.Common.Interfaces;
using Organization.Api.Domain.Entities;

namespace Organization.Api.Infrastructure.Persistance
{
    public class UserDatabaseContext(ICurrentUserService userService, DbContextOptions options) : ShadowContext(userService, options)
    {
        public virtual DbSet<Domain.Entities.User> Users { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<Permission> Permissions { get; set; }
        public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>()
                .HasMany(r => r.Permissions)
                .WithMany(p => p.Roles)
                .UsingEntity(j => j.ToTable("RolePermissions"));

            modelBuilder.Entity<Domain.Entities.User>()
                .HasMany(u => u.Roles)
                .WithMany(r => r.Users)
                .UsingEntity(j => j.ToTable("UserRoles"));

            base.OnModelCreating(modelBuilder);
        }

    }
}
