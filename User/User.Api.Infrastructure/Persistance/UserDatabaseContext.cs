using Microsoft.EntityFrameworkCore;
using User.Api.Domain.Entities;
using User.API.Common.Interfaces;

namespace User.Api.Infrastructure.Persistance
{
    public class UserDatabaseContext(ICurrentUserService userService, DbContextOptions options) : ShadowContext(userService, options)
    {
        public virtual DbSet<Domain.Entities.User> Users { get; set; }
        public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
        public virtual DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Domain.Entities.User>(entity =>
            {
                entity.HasIndex(x => x.Name).IsUnique();
                entity.HasIndex(x => x.Email).IsUnique();

                entity.Property(u => u.RowVersion)
                    .IsRowVersion()
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<Domain.Entities.User>()
                .HasMany(u => u.RefreshTokens)
                .WithOne()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Domain.Entities.User>()
                .HasMany(u => u.PasswordResetTokens)
                .WithOne()
                .HasForeignKey(prt => prt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RefreshToken>()
                .Property(rt => rt.Token)
                .IsRequired();
        }
    }
}
