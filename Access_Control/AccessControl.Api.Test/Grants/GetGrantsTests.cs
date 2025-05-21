using AccessControl.Api.Domain.Entities;
using AccessControl.Api.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using static AccessControl.Api.Features.Grants.GetGrantsController;

namespace AccessControl.Api.Test.Grants
{
    public class GetGrantsHandlerTests
    {
        private readonly Mock<Context> _dbContextMock;
        private readonly GetGrantsHandler _handler;

        public GetGrantsHandlerTests()
        {
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase("GetGrantsDbTest")
                .Options;

            var userServiceMock = new Mock<Common.Interfaces.ICurrentUserService>();
            _dbContextMock = new Mock<Context>(MockBehavior.Loose, userServiceMock.Object, options);
            _handler = new GetGrantsHandler(_dbContextMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnGrants_WhenUserIdMatches()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var permission = new Permission("Read", "Allows reading");
            var grant = new Grant(userId, Guid.NewGuid(), permission);

            _dbContextMock.Setup(db => db.Grants)
                .ReturnsDbSet(new List<Grant> { grant }.AsQueryable());

            var query = new GetGrantsQuery(userId, null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.Single(result.Value);
            Assert.Equal(userId, result.Value[0].UserId);
        }

        [Fact]
        public async Task Handle_ShouldFilterByResourceId_WhenProvided()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var resourceId1 = Guid.NewGuid();
            var resourceId2 = Guid.NewGuid();
            var permission = new Permission("Write", "Allows writing");

            var grant1 = new Grant(userId, resourceId1, permission);
            var grant2 = new Grant(userId, resourceId2, permission);

            _dbContextMock.Setup(db => db.Grants)
                .ReturnsDbSet(new List<Grant> { grant1, grant2 }.AsQueryable());

            var query = new GetGrantsQuery(userId, resourceId1);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.Single(result.Value);
            Assert.Equal(resourceId1, result.Value[0].ResourceId);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenNoGrantsFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();

            _dbContextMock.Setup(db => db.Grants)
                .ReturnsDbSet(new List<Grant>().AsQueryable());

            var query = new GetGrantsQuery(userId, resourceId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.Empty(result.Value);
        }
    }
}