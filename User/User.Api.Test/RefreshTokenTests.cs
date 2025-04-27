using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using User.Api.Common.Interfaces;
using User.Api.Features.Authentication;
using User.Api.Infrastructure.Persistance;
using User.Api.Infrastructure.Services;
using User.API.Common.Interfaces;

namespace User.Api.Test
{
    public class RefreshTokenTests
    {
        private readonly Mock<UserDatabaseContext> _dbContextMock;
        private readonly Mock<ICurrentUserService> _userServiceMock;
        private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
        private readonly Mock<IRefreshTokenGenerator> _refreshTokenGeneratorMock;
        private readonly Mock<IHashingService> _hashingServiceMock;

        public RefreshTokenTests()
        {

            _userServiceMock = new Mock<ICurrentUserService>();
            _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
            _refreshTokenGeneratorMock = new Mock<IRefreshTokenGenerator>();
            _hashingServiceMock = new Mock<IHashingService>();

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
        public async Task Handle_ShouldReturnError_WhenRefreshTokenIsInvalid()
        {
            // Arrange
            var invalidRefreshToken = "invalid-refresh-token";
            var query = new RefreshTokenQuery(invalidRefreshToken);
            var handler = new RefreshTokenQueryHandler(
                _dbContextMock.Object,
                _jwtTokenGeneratorMock.Object,
                _refreshTokenGeneratorMock.Object,
                _hashingServiceMock.Object
            );

            _dbContextMock.Setup(x => x.RefreshTokens)
                .ReturnsDbSet(new List<Domain.Entities.RefreshToken>());

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Invalid refresh token", result.FirstError.Code);
        }

        [Fact]
        public async Task Handle_ShouldReturnError_WhenUserNotFoundForRefreshToken()
        {
            // Arrange
            var refreshToken = "valid-refresh-token";
            var query = new RefreshTokenQuery(refreshToken);
            var handler = new RefreshTokenQueryHandler(
                _dbContextMock.Object,
                _jwtTokenGeneratorMock.Object,
                _refreshTokenGeneratorMock.Object,
                _hashingServiceMock.Object
            );

            var refreshTokenEntity = new Domain.Entities.RefreshToken
            {
                Token = refreshToken,
                UserId = Guid.NewGuid()
            };

            _dbContextMock.Setup(x => x.RefreshTokens)
                .ReturnsDbSet(new List<Domain.Entities.RefreshToken> { refreshTokenEntity }); 

            _dbContextMock.Setup(x => x.Users)
                .ReturnsDbSet(Enumerable.Empty<Domain.Entities.User>());

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsError);
            Assert.Equal("Invalid refresh token", result.FirstError.Code);
        }

        [Fact]
        public async Task Handle_ShouldReturnNewTokens_WhenRefreshTokenIsValid()
        {
            // Arrange
            var refreshToken = "valid-refresh-token";
            var user = new Domain.Entities.User("Test User", "test@example.com", "hashedpassword");
            var refreshTokenEntity = new Domain.Entities.RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id
            };

            var newAccessToken = "new-access-token";
            var newRefreshToken = "new-refresh-token";
            var query = new RefreshTokenQuery(refreshToken);
            var handler = new RefreshTokenQueryHandler(
                _dbContextMock.Object,
                _jwtTokenGeneratorMock.Object,
                _refreshTokenGeneratorMock.Object,
                _hashingServiceMock.Object
            );

            // Setup mocks
            _dbContextMock.Setup(x => x.RefreshTokens)
                .ReturnsDbSet(new List<Domain.Entities.RefreshToken> { refreshTokenEntity });
            _dbContextMock.Setup(x => x.Users)
                .ReturnsDbSet(new List<Domain.Entities.User> { user });
            _jwtTokenGeneratorMock.Setup(x => x.GenerateToken(It.IsAny<Domain.Entities.User>()))
                                  .Returns(newAccessToken);
            _refreshTokenGeneratorMock.Setup(x => x.GenerateAndStoreRefreshTokenAsync(It.IsAny<Domain.Entities.User>()))
                                       .ReturnsAsync(newRefreshToken);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsError);
            Assert.Equal(newAccessToken, result.Value.AccessToken);
            Assert.Equal(newRefreshToken, result.Value.RefreshToken);
        }
    }
}