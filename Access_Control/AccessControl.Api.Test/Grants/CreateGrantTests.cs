using AccessControl.Api.Infrastructure.Persistance;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using static AccessControl.Api.Features.Grants.CreateGrantController;

namespace AccessControl.Api.Test.Grants
{
    public class CreateGrantHandlerTests
    {
        private readonly Context _dbContext;
        private readonly CreateGrantHandler _handler;

        public CreateGrantHandlerTests()
        {
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Isolated DB per test
                .Options;

            var userServiceMock = new Mock<AccessControl.Api.Common.Interfaces.ICurrentUserService>();
            _dbContext = new Context(userServiceMock.Object, options);
            _handler = new CreateGrantHandler(_dbContext);
        }

        [Fact]
        public async Task Handle_ShouldReturnUnit_WhenGrantIsCreatedSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();
            var permission = "read";

            var command = new CreateGrantCommand(userId, resourceId, permission);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(Unit.Value, result.Value);

            var grantInDb = await _dbContext.Grants
                .FirstOrDefaultAsync(g => g.UserId == userId && g.ResourceId == resourceId && g.Permission == permission);

            Assert.NotNull(grantInDb);
        }

        [Fact]
        public async Task Handle_ShouldPersistGrant_WithCorrectValues()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();
            var permission = "write";

            var command = new CreateGrantCommand(userId, resourceId, permission);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            var savedGrant = await _dbContext.Grants.FirstOrDefaultAsync();
            Assert.NotNull(savedGrant);
            Assert.Equal(userId, savedGrant.UserId);
            Assert.Equal(resourceId, savedGrant.ResourceId);
            Assert.Equal(permission, savedGrant.Permission);
        }
    }
}