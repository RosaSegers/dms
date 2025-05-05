using AccessControl.Api.Domain.Entities;
using AccessControl.Api.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using Moq;
using static AccessControl.Api.Features.Grants.GetGrantsController;

namespace AccessControl.Api.Test.Grants
{
    public class GetGrantsHandlerTests
    {
        private readonly Context _dbContext;
        private readonly GetGrantsHandler _handler;

        public GetGrantsHandlerTests()
        {
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var userServiceMock = new Mock<AccessControl.Api.Common.Interfaces.ICurrentUserService>();
            _dbContext = new Context(userServiceMock.Object, options);
            _handler = new GetGrantsHandler(_dbContext);
        }

        [Fact]
        public async Task Handle_ShouldReturnAllGrants_ForGivenUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();

            var grants = new List<Grant>
            {
                new(userId, Guid.NewGuid(), "read"),
                new(userId, Guid.NewGuid(), "write"),
                new(Guid.NewGuid(), Guid.NewGuid(), "admin") // Belongs to another user
            };

            _dbContext.Grants.AddRange(grants);
            await _dbContext.SaveChangesAsync();

            var query = new GetGrantsQuery(userId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(2, result.Value.Count);
            Assert.All(result.Value, g => Assert.Equal(userId, g.UserId));
        }

        [Fact]
        public async Task Handle_ShouldReturnFilteredGrants_WhenResourceIdIsProvided()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();

            var grants = new List<Grant>
            {
                new(userId, resourceId, "read"),
                new(userId, Guid.NewGuid(), "write")
            };

            _dbContext.Grants.AddRange(grants);
            await _dbContext.SaveChangesAsync();

            var query = new GetGrantsQuery(userId, resourceId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.Single(result.Value);
            Assert.Equal(resourceId, result.Value[0].ResourceId);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenNoGrantsExistForUser()
        {
            // Arrange
            var query = new GetGrantsQuery(Guid.NewGuid());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.Empty(result.Value);
        }
    }
}