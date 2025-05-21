using AccessControl.Api.Common.Interfaces;
using AccessControl.Api.Domain.Entities;
using AccessControl.Api.Features.Permission;
using AccessControl.Api.Infrastructure.Persistance;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;

namespace AccessControl.Api.Test.Permissions
{
    public class GetUserPermissionQueryHandlerTests
    {
        private readonly Mock<Context> _dbContextMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ICurrentUserService> _userServiceMock;
        private readonly GetUserPermissionQueryHandler _handler;

        public GetUserPermissionQueryHandlerTests()
        {
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase("GetUserPermissionsDbTest")
                .Options;

            _userServiceMock = new Mock<ICurrentUserService>();
            _dbContextMock = new Mock<Context>(MockBehavior.Loose, _userServiceMock.Object, options);
            _mapperMock = new Mock<IMapper>();

            _handler = new GetUserPermissionQueryHandler(_dbContextMock.Object, _mapperMock.Object, _userServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnPermissions_WhenDataExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userServiceMock.Setup(x => x.UserId).Returns(userId.ToString());

            var grants = new List<Grant>
            {
                new Grant(userId, Guid.NewGuid(), new Permission("Read", "read"))
            };

            var assignments = new List<Assignment>
            {
                new Assignment(userId, Guid.NewGuid(), new Role("Admin", new List<Permission>()))
            };

            var roles = new List<Role>
            {
                new Role("Admin", new List<Permission> { new Permission("Write", "write") }) { Users = new List<Domain.Entities.User> { new TestUser(userId) } }
            };

            _dbContextMock.Setup(db => db.Grants)
                          .ReturnsDbSet(grants.AsQueryable());
            _dbContextMock.Setup(db => db.Assignment)
                          .ReturnsDbSet(assignments.AsQueryable());
            _dbContextMock.Setup(db => db.Roles)
                          .ReturnsDbSet(roles.AsQueryable());

            var query = new GetUserPermissionQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(3, result.Value.Count); // 1 Grant + 1 Assignment + 1 Role
        }

        [Fact]
        public async Task Handle_ShouldThrowUnauthorized_WhenUserIdIsNull()
        {
            // Arrange
            _userServiceMock.Setup(x => x.UserId).Returns<string?>(null);

            var query = new GetUserPermissionQuery();

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                await _handler.Handle(query, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenNoDataExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userServiceMock.Setup(x => x.UserId).Returns(userId.ToString());

            _dbContextMock.Setup(db => db.Grants).ReturnsDbSet(new List<Grant>().AsQueryable());
            _dbContextMock.Setup(db => db.Assignment).ReturnsDbSet(new List<Assignment>().AsQueryable());
            _dbContextMock.Setup(db => db.Roles).ReturnsDbSet(new List<Role>().AsQueryable());

            var query = new GetUserPermissionQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.Empty(result.Value);
        }

        // Helper class to mock abstract User
        private class TestUser : Domain.Entities.User
        {
            public TestUser(Guid id) => Id = id;
        }
    }
}