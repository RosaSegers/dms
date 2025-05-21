using AccessControl.Api.Common.Interfaces;
using AccessControl.Api.Domain.Entities;
using AccessControl.Api.Infrastructure.Persistance;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using static AccessControl.Api.Features.Grants.CreateGrantController;

namespace AccessControl.Api.Test.Grants
{
    public class CreateGrantHandlerTests
    {
        private readonly Mock<Context> _dbContextMock;
        private readonly CreateGrantHandler _handler;
        private readonly Mock<ICurrentUserService> _userServiceMock;

        public CreateGrantHandlerTests()
        {
            _userServiceMock = new Mock<ICurrentUserService>();

            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase("GrantDbTest")
                .Options;

            _dbContextMock = new Mock<Context>(_userServiceMock.Object, options);
            _handler = new CreateGrantHandler(_dbContextMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldCreateGrant_WhenPermissionExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();
            var permissionName = "Read";

            var permission = new Permission(permissionName, "Allows read access");
            var command = new CreateGrantCommand(userId, resourceId, permissionName);

            var permissions = new List<Permission> { permission }.AsQueryable();
            var grants = new List<Grant>();

            _dbContextMock.Setup(db => db.Permissions)
                          .ReturnsDbSet(permissions);

            _dbContextMock.Setup(db => db.Grants)
                          .ReturnsDbSet(grants.AsQueryable());

            _dbContextMock.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
                          .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(Unit.Value, result.Value);

            _dbContextMock.Verify(db => db.Grants.Add(It.Is<Grant>(
                g => g.UserId == userId &&
                     g.ResourceId == resourceId &&
                     g.Permission == permission
            )), Times.Once);

            _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowInvalidOperation_WhenPermissionNotFound()
        {
            // Arrange
            var command = new CreateGrantCommand(Guid.NewGuid(), Guid.NewGuid(), "NonExistent");

            _dbContextMock.Setup(db => db.Permissions)
                          .ReturnsDbSet(new List<Permission>().AsQueryable());

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _handler.Handle(command, CancellationToken.None));
        }
    }
}