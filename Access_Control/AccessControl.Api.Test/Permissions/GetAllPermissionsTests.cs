using AccessControl.Api.Domain.Entities;
using AccessControl.Api.Features.Permission;
using AccessControl.Api.Infrastructure.Persistance;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;

namespace AccessControl.Api.Test.Permissions
{
    public class GetPermissionsQueryHandlerTests
    {
        private readonly Mock<Context> _dbContextMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetPermissionsQueryHandler _handler;

        public GetPermissionsQueryHandlerTests()
        {
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase("PermissionsDbTest")
                .Options;

            var userServiceMock = new Mock<Common.Interfaces.ICurrentUserService>();
            _dbContextMock = new Mock<Context>(MockBehavior.Loose, userServiceMock.Object, options);
            _mapperMock = new Mock<IMapper>();

            _handler = new GetPermissionsQueryHandler(_dbContextMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnMappedPermissions_WhenPermissionsExist()
        {
            // Arrange
            var permissions = new List<Permission>
            {
                new Permission("Read", "Allows read access"),
                new Permission("Write", "Allows write access")
            };

            var permissionDtos = new List<Domain.Dtos.Permission>
            {
                new Domain.Dtos.Permission { Name = "Read", Description = "Allows read access" },
                new Domain.Dtos.Permission { Name = "Write", Description = "Allows write access" }
            };

            _dbContextMock.Setup(db => db.Permissions)
                .ReturnsDbSet(permissions.AsQueryable());

            _mapperMock.Setup(m => m.Map<List<Domain.Dtos.Permission>>(It.IsAny<List<Permission>>()))
                .Returns(permissionDtos);

            var query = new GetPermissionsQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(2, result.Value.Count);
            Assert.Equal("Read", result.Value[0].Name);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenNoPermissionsExist()
        {
            // Arrange
            _dbContextMock.Setup(db => db.Permissions)
                .ReturnsDbSet(new List<Permission>().AsQueryable());

            var query = new GetPermissionsQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.Empty(result.Value);
            _mapperMock.Verify(m => m.Map<List<Domain.Dtos.Permission>>(It.IsAny<List<Permission>>()), Times.Never);
        }
    }
}