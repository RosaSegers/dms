using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using User.API.Common.Interfaces;
using User.API.Common.Interfaces.Markers;

namespace User.Api.Infrastructure.Persistance
{
    public abstract class ShadowContext(ICurrentUserService userService, DbContextOptions options) : DbContext(options)
    {
        private readonly string? _user = userService.UserId;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            AddTrackingDatesShadowProperties(modelBuilder);
            AddSoftDeleteShadowProperties(modelBuilder);
            AddUserDataProperties(modelBuilder);
        }

        public override int SaveChanges()
        {
            //UpdateTrackingDates();
            UpdateSoftDeleteProperties();
            if (_user != null)
                UpdateUserData();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = new CancellationToken())
        {
            //UpdateTrackingDates();
            UpdateSoftDeleteProperties();
            if (_user != null)
                UpdateUserData();
            return base.SaveChangesAsync(cancellationToken);
        }

        #region AddShadowProperties
        public static void AddTrackingDatesShadowProperties(ModelBuilder modelBuilder)
        {
            var trackingDatesEntities = typeof(IHaveTrackingData)
                .Assembly.GetTypes()
                    .Where(
                        type => typeof(IHaveTrackingData)
                            .IsAssignableFrom(type)
                            && type.IsClass
                            && !type.IsAbstract);

            foreach (var entity in trackingDatesEntities)
            {
                modelBuilder.Entity(entity)
                    .Property<DateTimeOffset>("CreatedOn")
                    .IsRequired();
                modelBuilder.Entity(entity)
                    .Property<DateTimeOffset>("LastUpdatedOn")
                    .IsRequired()
                    .HasDefaultValue(DateTimeOffset.MinValue)
                    .HasColumnName("LastUpdatedAt");
            }
        }

        public static void AddSoftDeleteShadowProperties(ModelBuilder modelBuilder)
        {
            var softDeleteEntities = typeof(ICanBeSoftDeleted)
                .Assembly.GetTypes()
                    .Where(
                        type => typeof(ICanBeSoftDeleted)
                            .IsAssignableFrom(type)
                            && type.IsClass
                            && !type.IsAbstract);

            foreach (var entity in softDeleteEntities)
            {
                modelBuilder.Entity(entity)
                    .Property<bool>("IsDeleted")
                    .HasDefaultValue(false);
            }
        }

        public static void AddUserDataProperties(ModelBuilder modelBuilder)
        {
            var userDataEntities = typeof(IHaveUserData)
                .Assembly.GetTypes()
                    .Where(
                        type => typeof(IHaveUserData)
                            .IsAssignableFrom(type)
                            && type.IsClass
                            && !type.IsAbstract);

            foreach (var entity in userDataEntities)
            {
                modelBuilder.Entity(entity)
                    .Property<string>("CreatedBy")
                    .HasDefaultValue(false);
                modelBuilder.Entity(entity)
                    .Property<string>("LastUpdatedBy")
                    .HasDefaultValue(false);
            }
        }
        #endregion
        #region UpdateShadowProperties
        private void UpdateTrackingDates()
        {
            var trackingDatesChangedEntries = ChangeTracker.Entries()
            .Where(entry => (entry.State == EntityState.Added ||
                                entry.State == EntityState.Modified) && entry.Entity is IHaveTrackingData);

            foreach (var entry in trackingDatesChangedEntries)
            {
                if (entry.State == EntityState.Modified)
                    Entry(entry).Property("LastUpdatedOn")
                        .CurrentValue = DateTimeOffset.UtcNow;
                else if (entry.State == EntityState.Added)
                    Entry(entry).Property("CreatedOn")
                        .CurrentValue = DateTimeOffset.UtcNow;
            }
        }

        private void UpdateSoftDeleteProperties()
        {
            var softDeleteChangedEntries = ChangeTracker.Entries()
                .Where(entry => entry.State == EntityState.Deleted && entry.Entity is ICanBeSoftDeleted);

            foreach (var entry in softDeleteChangedEntries)
            {
                Entry(entry).Property("IsDeleted").CurrentValue = true;
                Entry(entry).State = EntityState.Detached;
            }
        }

        private void UpdateUserData()
        {
            var userDataChangedEntries = ChangeTracker.Entries()
                .Where(entry => (entry.State == EntityState.Added ||
                                entry.State == EntityState.Modified) && entry.Entity is IHaveUserData);

            foreach (var entry in userDataChangedEntries)
            {
                if (entry.State == EntityState.Modified)
                    Entry(entry).Property("LastUpdatedBy")
                        .CurrentValue = _user;
                else if (entry.State == EntityState.Added)
                    Entry(entry).Property("CreatedBy")
                        .CurrentValue = _user;
            }
        }
        #endregion
    }
}
