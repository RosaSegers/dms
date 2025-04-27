using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using User.Api.Domain.Entities;
using User.Api.Infrastructure.Persistance;
using User.Api.Infrastructure.Services;
using User.API.Common.Interfaces;

namespace User.Api.Test
{

    public class RefreshTokenGeneratorTests
    {
        private static UserDatabaseContext CreateInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<UserDatabaseContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            var mockUserService = new Mock<ICurrentUserService>();
            mockUserService.Setup(x => x.UserId).Returns(Guid.NewGuid().ToString());

            return new UserDatabaseContext(mockUserService.Object, options);
        }

        [Fact]
        public void GenerateRefreshToken_ShouldReturnValidBase64()
        {
            var db = CreateInMemoryDbContext("TestDb_TokenGen");
            var generator = new RefreshTokenGenerator(db);

            var token = generator.GenerateRefreshToken();

            Assert.False(string.IsNullOrWhiteSpace(token));
            var bytes = Convert.FromBase64String(token);
            Assert.Equal(64, bytes.Length);
        }

        [Fact]
        public async Task GenerateAndStoreRefreshTokenAsync_ShouldStoreNewToken()
        {
            var db = CreateInMemoryDbContext("TestDb_StoreToken");
            var generator = new RefreshTokenGenerator(db);
            var user = new Domain.Entities.User("alice", "alice@example.com", "pass") { Id = Guid.NewGuid() };

            var token = await generator.GenerateAndStoreRefreshTokenAsync(user);

            var storedToken = db.RefreshTokens.FirstOrDefault(rt => rt.Token == token);
            Assert.NotNull(storedToken);
            Assert.Equal(user.Id, storedToken.UserId);
            Assert.False(storedToken.IsRevoked);
            Assert.True(storedToken.ExpiresAt > DateTime.UtcNow);
        }

        [Fact]
        public async Task GenerateAndStoreRefreshTokenAsync_ShouldRevokeOldTokens()
        {
            var db = CreateInMemoryDbContext("TestDb_RevokeToken");
            var generator = new RefreshTokenGenerator(db);
            var user = new Domain.Entities.User("bob", "bob@example.com", "pass") { Id = Guid.NewGuid() };

            var oldToken = new RefreshToken
            {
                UserId = user.Id,
                Token = "old_token_value",
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                ExpiresAt = DateTime.UtcNow.AddDays(2),
                IsRevoked = false
            };

            db.RefreshTokens.Add(oldToken);
            await db.SaveChangesAsync();

            var newToken = await generator.GenerateAndStoreRefreshTokenAsync(user);

            var revokedToken = db.RefreshTokens.First(rt => rt.Token == "old_token_value");
            var storedToken = db.RefreshTokens.First(rt => rt.Token == newToken);

            Assert.True(revokedToken.IsRevoked);
            Assert.False(storedToken.IsRevoked);
            Assert.Equal(user.Id, storedToken.UserId);
        }
    }
}
