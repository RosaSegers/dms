using AccessControl.Api.Features.Permission;
using AccessControl.Api.Infrastructure.Persistance;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace AccessControl.Api.Test.Permissions
{
    public class GetPermissionsQueryHandlerTests
    {
        private readonly Context _dbContext;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetPermissionsQueryHandler _handler;

        public GetPermissionsQueryHandlerTests()
        {
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Unique DB per test
                .Options;

            var userServiceMock = new Mock<AccessControl.Api.Common.Interfaces.ICurrentUserService>();
            _dbContext = new Context(userServiceMock.Object, options);

            _mapperMock = new Mock<IMapper>();
            _handler = new GetPermissionsQueryHandler(_dbContext, _mapperMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnMappedPermissionDtos_WhenPermissionsExist()
        {
            // Arrange
            var permissionEntities = new List<Domain.Entities.Permission>
            {
                new Domain.Entities.Permission("read", "Can view content"),
                new Domain.Entities.Permission("write", "Can modify content")
            };

            // Ensure permissions are added to the database
            _dbContext.Permissions.AddRange(permissionEntities);
            await _dbContext.SaveChangesAsync();

            // Mock AutoMapper to return the DTOs we expect
            var expectedDtos = permissionEntities.Select(p => new Domain.Dtos.Permission
            {
                Name = p.Name,
                Description = p.Description
            }).ToList();

            _mapperMock
                .Setup(m => m.Map<List<Domain.Dtos.Permission>>(It.IsAny<List<Domain.Entities.Permission>>()))
                .Returns(expectedDtos);

            var query = new GetPermissionsQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(2, result.Value.Count);
            Assert.Equal("read", result.Value[0].Name);
            Assert.Equal("Can view content", result.Value[0].Description);
            Assert.Equal("write", result.Value[1].Name);
            Assert.Equal("Can modify content", result.Value[1].Description);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenNoPermissionsExist()
        {
            // Arrange
            _mapperMock
                .Setup(m => m.Map<List<Domain.Entities.Permission>>(It.IsAny<List<Domain.Entities.Permission>>()))
                .Returns(new List<Domain.Entities.Permission>());

            var query = new GetPermissionsQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.Empty(result.Value);
        }
    }
}