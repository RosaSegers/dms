namespace User.Api.Test.Unit
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using global::User.Api.Features.Users;
    using global::User.Api.Infrastructure.Persistance;
    using global::User.API.Common.Interfaces;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using Xunit;

    namespace User.Api.Test.Unit
    {
        // Subclass UserDatabaseContext to override SaveChangesAsync for concurrency exception simulation
        public class TestUserDatabaseContext : UserDatabaseContext
        {
            private int _saveChangesCallCount = 0;
            public int MaxConcurrencyExceptions { get; set; } = 3;

            public TestUserDatabaseContext(DbContextOptions<UserDatabaseContext> options, ICurrentUserService userService)
                : base(userService, options)
            { }

            public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            {
                _saveChangesCallCount++;
                if (_saveChangesCallCount <= MaxConcurrencyExceptions)
                {
                    throw new DbUpdateConcurrencyException();
                }
                return base.SaveChangesAsync(cancellationToken);
            }
        }

        public class UpdateUserCommandHandlerTests
        {
            private readonly TestUserDatabaseContext _dbContext;
            private readonly Mock<ICurrentUserService> _userServiceMock;
            private readonly UpdateUserCommandHandler _handler;

            public UpdateUserCommandHandlerTests()
            {
                var options = new DbContextOptionsBuilder<UserDatabaseContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // unique DB per test run
                    .Options;

                _userServiceMock = new Mock<ICurrentUserService>();

                _dbContext = new TestUserDatabaseContext(options, _userServiceMock.Object);

                _handler = new UpdateUserCommandHandler(_dbContext, _userServiceMock.Object);
            }


            [Fact]
            public async Task Handle_ShouldUpdateUser_WhenNoConcurrencyIssues()
            {
                // Arrange
                var userId = Guid.NewGuid();
                var user = new Domain.Entities.User(userId, "OldName", "oldemail@example.com", "hashedPassword")
                {
                    RowVersion = new byte[] { 1, 2, 3 }
                };

                _userServiceMock.Setup(u => u.UserId).Returns(userId);

                // Disable concurrency exceptions for this test
                _dbContext.MaxConcurrencyExceptions = 0;

                await _dbContext.Users.AddAsync(user);
                await _dbContext.SaveChangesAsync();

                var command = new UpdateUserCommand(DateTime.UtcNow, "NewName", "newemail@example.com");

                // Act
                var result = await _handler.Handle(command, CancellationToken.None);

                // Assert
                Assert.False(result.IsError);

                var updatedUser = await _dbContext.Users.FindAsync(userId);
                Assert.Equal("NewName", updatedUser.Name);
                Assert.Equal("newemail@example.com", updatedUser.Email);
            }
        }
    }
}
