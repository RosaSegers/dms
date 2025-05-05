using AccessControl.Api.Domain.Entities;
using AccessControl.Api.Infrastructure.Persistance;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Moq;
using static AccessControl.Api.Features.Grants.DeleteGrantController;

namespace AccessControl.Api.Test.Grants
{
    public class DeleteGrantHandlerTests
    {
        private readonly Context _dbContext;
        private readonly DeleteGrantHandler _handler;

        public DeleteGrantHandlerTests()
        {
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Isolated for each test
                .Options;

            var userServiceMock = new Mock<AccessControl.Api.Common.Interfaces.ICurrentUserService>();
            _dbContext = new Context(userServiceMock.Object, options);
            _handler = new DeleteGrantHandler(_dbContext);
        }

        [Fact]
        public async Task Handle_ShouldReturnDeleted_WhenGrantIsDeletedSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();
            var permission = "read";

            var grant = new Grant(userId, resourceId, permission);
            _dbContext.Grants.Add(grant);
            await _dbContext.SaveChangesAsync();

            var command = new DeleteGrantCommand(userId, resourceId, permission);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(Result.Deleted, result.Value);

            var grantInDb = await _dbContext.Grants
                .FirstOrDefaultAsync(g => g.UserId == userId && g.ResourceId == resourceId && g.Permission == permission);

            Assert.Null(grantInDb);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFound_WhenGrantDoesNotExist()
        {
            // Arrange
            var command = new DeleteGrantCommand(Guid.NewGuid(), Guid.NewGuid(), "write");

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Grant.NotFound", result.FirstError.Code);
            Assert.Equal("The specified permission grant does not exist.", result.FirstError.Description);
            Assert.Equal(ErrorType.NotFound, result.FirstError.Type);
        }
    }
}