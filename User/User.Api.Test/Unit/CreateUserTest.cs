using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using User.Api.Common.Interfaces;
using User.Api.Features.Users;
using User.Api.Infrastructure.Persistance;
using User.API.Common.Interfaces;

namespace User.Api.Test.Unit
{
    public class CreateUserTest
    {
        private readonly Mock<IHashingService> _hashingServiceMock;
        private readonly Mock<UserDatabaseContext> _dbContextMock;
        private readonly Mock<ICurrentUserService> _userService;
        private readonly CreateUserCommandHandler _handler;

        public CreateUserTest()
        {
            _hashingServiceMock = new();
            _userService = new();

            var optionsBuilder = new DbContextOptionsBuilder<UserDatabaseContext>();
            optionsBuilder.UseInMemoryDatabase("Users");

            _dbContextMock = new(_userService.Object, optionsBuilder.Options);
            _handler = new CreateUserCommandHandler(_hashingServiceMock.Object, _dbContextMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnUserId_WhenUserIsCreatedSuccessfully()
        {
            // Arrange
            var query = new CreateUserCommand("testuser", "testuser@example.com", "TestPassword123");
            var userId = Guid.NewGuid();
            var user = new Domain.Entities.User(query.username, query.email, "hashedPassword123")
            {
                Id = userId
            };

            // Mock the database context
            var dbSetMock = new Mock<DbSet<Domain.Entities.User>>();
            _dbContextMock.Setup(db => db.Users).Returns(dbSetMock.Object);

            // Mock Hashing Service
            _hashingServiceMock.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashedPassword123");

            dbSetMock.Setup(d => d.AddAsync(It.IsAny<Domain.Entities.User>(), It.IsAny<CancellationToken>()));
            _dbContextMock.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.IsType<Guid>(result.Value);
            Assert.True(result.Value != Guid.Empty);
            Assert.False(result.IsError);
        }

        [Fact]
        public async Task Handle_ShouldReturnError_WhenHashingFails()
        {
            // Arrange
            var query = new CreateUserCommand("testuser", "testuser@example.com", "TestPassword123");

            // Mock the database context
            var dbSetMock = new Mock<DbSet<Domain.Entities.User>>();
            _dbContextMock.Setup(db => db.Users).Returns(dbSetMock.Object);

            // Mock Hashing Service to throw an exception
            _hashingServiceMock.Setup(h => h.Hash(It.IsAny<string>())).Throws(new Exception("Hashing error"));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsError);
            Assert.True(result.Value == Guid.Empty);
            Assert.Equal("Hashing error", result.Errors.First().Code);
        }
    }
}
