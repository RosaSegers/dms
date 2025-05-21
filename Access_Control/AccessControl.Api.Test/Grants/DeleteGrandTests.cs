using AccessControl.Api.Common.Interfaces;
using AccessControl.Api.Domain.Entities;
using AccessControl.Api.Infrastructure.Persistance;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using static AccessControl.Api.Features.Grants.DeleteGrantController;

namespace AccessControl.Api.Test.Grants
{
    public class DeleteGrantHandlerTests
    {
        private readonly Mock<Context> _dbContextMock;
        private readonly DeleteGrantHandler _handler;

        public DeleteGrantHandlerTests()
        {
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase("DeleteGrantDbTest")
                .Options;

            var userServiceMock = new Mock<ICurrentUserService>();

            _dbContextMock = new Mock<Context>(MockBehavior.Loose, userServiceMock.Object, options);
            _handler = new DeleteGrantHandler(_dbContextMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnDeleted_WhenGrantExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();
            var permissionName = "Read";
            var permission = new Permission(permissionName, "Allows reading");
            var grant = new Grant(userId, resourceId, permission);

            var grants = new List<Grant> { grant };

            _dbContextMock.Setup(db => db.Grants)
                          .ReturnsDbSet(grants.AsQueryable());

            _dbContextMock.Setup(db => db.Grants.Remove(It.IsAny<Grant>()));
            _dbContextMock.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
                          .ReturnsAsync(1);

            var command = new DeleteGrantCommand(userId, resourceId, permissionName);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(Result.Deleted, result.Value);
            _dbContextMock.Verify(db => db.Grants.Remove(It.Is<Grant>(g =>
                g.UserId == userId &&
                g.ResourceId == resourceId &&
                g.Permission.Name == permissionName)), Times.Once);

            _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnError_WhenGrantNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();
            var permissionName = "Delete";

            _dbContextMock.Setup(db => db.Grants)
                          .ReturnsDbSet(new List<Grant>().AsQueryable());

            var command = new DeleteGrantCommand(userId, resourceId, permissionName);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Grant.NotFound", result.FirstError.Code);
            Assert.Equal("The specified permission grant does not exist.", result.FirstError.Description);

            _dbContextMock.Verify(db => db.Grants.Remove(It.IsAny<Grant>()), Times.Never);
            _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}