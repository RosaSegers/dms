using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Moq;
using Organization.Api.Common.Interfaces;
using Organization.Api.Features.Organization;
using Organization.Api.Infrastructure.Persistance;

namespace Organization.Api.Test
{
    public class CreateOrganizationTests
    {
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly DatabaseContext _dbContext;
        private readonly CreateOrganizationCommandHandler _handler;

        public CreateOrganizationTests()
        {
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbContext = new DatabaseContext(_currentUserServiceMock.Object, options);
            _handler = new CreateOrganizationCommandHandler(_dbContext, _currentUserServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldCreateOrganization_WhenValidUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _currentUserServiceMock.Setup(x => x.UserId).Returns(userId.ToString());

            var command = new CreateOrganizationCommand("Test Org", "TestOrgSlug");

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsError, "Expected result to be successful");
            Assert.NotEqual(Guid.Empty, result.Value);

            var orgInDb = await _dbContext.Organizations.FindAsync(result.Value);
            Assert.NotNull(orgInDb);
            Assert.Equal("Test Org", orgInDb!.Name);
            Assert.Equal(userId, orgInDb.OwnerId);
        }

        [Fact]
        public async Task Handle_ShouldReturnError_WhenUserIsNotAuthenticated()
        {
            // Arrange
            _currentUserServiceMock.Setup(x => x.UserId).Returns(Guid.Empty.ToString());
            var command = new CreateOrganizationCommand("Test Org", "TestOrgSlug");

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsError, "Expected result to be an error");
            Assert.Contains(result.Errors, e => e.Type == ErrorType.Unauthorized);
        }
    }
}
