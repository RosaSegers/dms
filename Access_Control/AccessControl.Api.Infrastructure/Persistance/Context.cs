using AccessControl.Api.Common.Interfaces;
using AccessControl.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AccessControl.Api.Infrastructure.Persistance
{
    public class Context(ICurrentUserService userService, DbContextOptions options) : ShadowContext(userService, options)
    {
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<Permission> Permissions { get; set; }
        public virtual DbSet<Assignment> Assignment { get; set; }
        public virtual DbSet<Grant> Grants { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>()
                .HasMany(r => r.Permissions)
                .WithMany(p => p.Roles)
                .UsingEntity(j => j.ToTable("RolePermissions"));

            modelBuilder.Entity<Assignment>()
                .HasKey(a => new { a.UserId, a.ResourceId, a.RoleId });

            modelBuilder.Entity<Grant>()
                .HasKey(g => new { g.UserId, g.ResourceId, g.Permission });

            base.OnModelCreating(modelBuilder);
        }

    }
}
