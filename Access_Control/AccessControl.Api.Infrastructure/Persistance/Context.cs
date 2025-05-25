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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the Role-Permission many-to-many relationship
            modelBuilder.Entity<Role>()
                .HasMany(r => r.Permissions)
                .WithMany(p => p.Roles)
                .UsingEntity(j => j.ToTable("RolePermissions"));

            modelBuilder.Entity<Role>()
                .HasMany(r => r.Users)
                .WithMany() 
                .UsingEntity("RoleUsers");
        }
    }
}