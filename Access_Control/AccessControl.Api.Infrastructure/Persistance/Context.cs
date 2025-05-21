using AccessControl.Api.Common.Interfaces;
using AccessControl.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AccessControl.Api.Infrastructure.Persistance
{
    public class Context : ShadowContext
    {
        private readonly ICurrentUserService _userService;

        public Context(ICurrentUserService userService, DbContextOptions options)
            : base(userService, options)
        {
            _userService = userService;
        }

        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<Permission> Permissions { get; set; }
        public virtual DbSet<Assignment> Assignment { get; set; }
        public virtual DbSet<Grant> Grants { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the Role-Permission many-to-many relationship
            modelBuilder.Entity<Role>()
                .HasMany(r => r.Permissions)
                .WithMany(p => p.Roles)
                .UsingEntity(j => j.ToTable("RolePermissions"));

            // Define the composite key for Assignment
            modelBuilder.Entity<Assignment>()
                .HasKey(a => new { a.UserId, a.ResourceId, a.Role.Id });

            // Define the composite key for Grant
            modelBuilder.Entity<Grant>()
                .HasKey(g => new { g.UserId, g.ResourceId, g.Permission });

            modelBuilder.Entity<Role>()
                .HasMany(r => r.Users)
                .WithMany() 
                .UsingEntity("RoleUsers");
        }
    }
}