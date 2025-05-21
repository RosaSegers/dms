using AccessControl.Api.Domain.Entities;
using AccessControl.Api.Features.Permission;
using AccessControl.Api.Infrastructure.Persistance;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;

namespace AccessControl.Api.Test.Permissions
{
    public class CheckUserPermissionQueryHandlerTests
    {
        private readonly Mock<Context> _dbContextMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly CheckUserPermissionQueryHandler _handler;

        public CheckUserPermissionQueryHandlerTests()
        {
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase("CheckUserPermissionDbTest")
                .Options;

            var userServiceMock = new Mock<Common.Interfaces.ICurrentUserService>();
            _dbContextMock = new Mock<Context>(MockBehavior.Loose, userServiceMock.Object, options);
            _mapperMock = new Mock<IMapper>();
            _handler = new CheckUserPermissionQueryHandler(_dbContextMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnTrue_WhenPermissionExistsInGrants()
        {
            // Arrange
            var permissionName = "Read";
            var userId = Guid.NewGuid();
            var documentId = Guid.NewGuid();

            var grants = new List<Grant>
            {
                new Grant(userId, documentId, new Permission(permissionName, "desc"))
            };

            _dbContextMock.Setup(db => db.Grants)
                .ReturnsDbSet(grants.AsQueryable());

            _dbContextMock.Setup(db => db.Assignment).ReturnsDbSet(new List<Assignment>().AsQueryable());
            _dbContextMock.Setup(db => db.Permissions).ReturnsDbSet(new List<Permission>().AsQueryable());
            _dbContextMock.Setup(db => db.Roles).ReturnsDbSet(new List<Role>().AsQueryable());

            var query = new CheckUserPermissionQuery(permissionName, userId, documentId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.True(result.Value);
        }

        [Fact]
        public async Task Handle_ShouldReturnTrue_WhenPermissionExistsViaRoleInPermissionsTable()
        {
            // Arrange
            var permissionName = "Write";
            var userId = Guid.NewGuid();
            var documentId = Guid.NewGuid();
            var roleId = Guid.NewGuid();

            var assignment = new Assignment(userId, documentId, new Role(roleId, "Editor", new List<Permission>()));
            var assignments = new List<Assignment> { assignment };

            var permission = new Permission(permissionName, "Write access");
            permission.Roles = new List<Role> { assignment.Role };

            _dbContextMock.Setup(db => db.Grants).ReturnsDbSet(new List<Grant>().AsQueryable());
            _dbContextMock.Setup(db => db.Assignment).ReturnsDbSet(assignments.AsQueryable());
            _dbContextMock.Setup(db => db.Permissions).ReturnsDbSet(new List<Permission> { permission }.AsQueryable());
            _dbContextMock.Setup(db => db.Roles).ReturnsDbSet(new List<Role>().AsQueryable());

            var query = new CheckUserPermissionQuery(permissionName, userId, documentId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.True(result.Value);
        }

        [Fact]
        public async Task Handle_ShouldReturnTrue_WhenPermissionExistsInIncludedRoles()
        {
            // Arrange
            var permissionName = "Delete";
            var userId = Guid.NewGuid();
            var documentId = Guid.NewGuid();
            var roleId = Guid.NewGuid();

            var permission = new Permission(permissionName, "Delete access");
            var role = new Role(roleId, "Admin", new List<Permission> { permission });

            var assignment = new Assignment(userId, documentId, role);

            _dbContextMock.Setup(db => db.Grants).ReturnsDbSet(new List<Grant>().AsQueryable());
            _dbContextMock.Setup(db => db.Assignment).ReturnsDbSet(new List<Assignment> { assignment }.AsQueryable());
            _dbContextMock.Setup(db => db.Permissions).ReturnsDbSet(new List<Permission>().AsQueryable());
            _dbContextMock.Setup(db => db.Roles).ReturnsDbSet(new List<Role> { role }.AsQueryable());

            var query = new CheckUserPermissionQuery(permissionName, userId, documentId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.True(result.Value);
        }

        [Fact]
        public async Task Handle_ShouldReturnFalse_WhenPermissionIsNotFoundAnywhere()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var documentId = Guid.NewGuid();
            var permissionName = "Execute";

            _dbContextMock.Setup(db => db.Grants).ReturnsDbSet(new List<Grant>().AsQueryable());
            _dbContextMock.Setup(db => db.Assignment).ReturnsDbSet(new List<Assignment>().AsQueryable());
            _dbContextMock.Setup(db => db.Permissions).ReturnsDbSet(new List<Permission>().AsQueryable());
            _dbContextMock.Setup(db => db.Roles).ReturnsDbSet(new List<Role>().AsQueryable());

            var query = new CheckUserPermissionQuery(permissionName, userId, documentId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.False(result.Value);
        }
    }
}