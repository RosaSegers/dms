using Moq;
using User.Api.Features.Users;
using User.Api.Infrastructure.Persistance;
using User.API.Common.Interfaces;
using Xunit;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Moq.EntityFrameworkCore;
using User.Api.Common.Interfaces;
using User.Api.Features.Authentication;
using User.Api.Infrastructure.Services;

namespace User.Api.Test.Unit
{
    public class LoginTests
    {
        private readonly Mock<UserDatabaseContext> _dbContextMock;
        private readonly Mock<ICurrentUserService> _userServiceMock;
        private readonly Mock<IHashingService> _hashingServiceMock;
        private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
        private readonly Mock<IRefreshTokenGenerator> _refreshTokenGeneratorMock;

        public LoginTests()
        {
            _userServiceMock = new Mock<ICurrentUserService>();
            _hashingServiceMock = new Mock<IHashingService>();
            _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
            _refreshTokenGeneratorMock = new Mock<IRefreshTokenGenerator>();

            var users = new List<Domain.Entities.User>
            {
                new Domain.Entities.User("Rosa1", "Rosa1@Email.com", "HashedPassword1"),
                new Domain.Entities.User("Rosa2", "Rosa2@Email.com", "HashedPassword2")
            };

            var optionsBuilder = new DbContextOptionsBuilder<UserDatabaseContext>();
            optionsBuilder.UseInMemoryDatabase("Users");
            _dbContextMock = new Mock<UserDatabaseContext>(_userServiceMock.Object, optionsBuilder.Options);
            _dbContextMock.Setup(p => p.Users).ReturnsDbSet(users.AsQueryable());
        }

        [Fact]
        public async Task Handle_ShouldReturnError_WhenInvalidPassword()
        {
            // Arrange
            var command = new LoginQuery("Rosa1@Email.com", "WrongPassword");
            var handler = new LoginQueryHandler(
                _dbContextMock.Object,
                _jwtTokenGeneratorMock.Object,
                _refreshTokenGeneratorMock.Object,
                _hashingServiceMock.Object
            );

            _hashingServiceMock.Setup(h => h.Validate(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Invalid credentials.", result.FirstError.Code);
        }

        [Fact]
        public async Task Handle_ShouldReturnTokens_WhenValidCredentials()
        {
            // Arrange
            var command = new LoginQuery("Rosa1@Email.com", "CorrectPassword");
            var handler = new LoginQueryHandler(
                _dbContextMock.Object,
                _jwtTokenGeneratorMock.Object,
                _refreshTokenGeneratorMock.Object,
                _hashingServiceMock.Object
            );

            _hashingServiceMock.Setup(h => h.Validate(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            var accessToken = "AccessToken123";
            var refreshToken = "RefreshToken123";

            _jwtTokenGeneratorMock.Setup(jwt => jwt.GenerateToken(It.IsAny<Domain.Entities.User>())).Returns(accessToken);
            _refreshTokenGeneratorMock.Setup(r => r.GenerateAndStoreRefreshTokenAsync(It.IsAny<Domain.Entities.User>())).ReturnsAsync(refreshToken);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(accessToken, result.Value.AccessToken);
            Assert.Equal(refreshToken, result.Value.RefreshToken);
        }

        [Fact]
        public async Task Handle_ShouldReturnError_WhenUserNotFound()
        {
            // Arrange
            var command = new LoginQuery("NonExistentUser@Email.com", "AnyPassword");
            var handler = new LoginQueryHandler(
                _dbContextMock.Object,
                _jwtTokenGeneratorMock.Object,
                _refreshTokenGeneratorMock.Object,
                _hashingServiceMock.Object
            );

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Invalid credentials.", result.FirstError.Code);
        }
    }
}