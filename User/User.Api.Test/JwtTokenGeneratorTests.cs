using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using User.Api.Infrastructure.Services;

namespace User.Api.Test
{
    public class JwtTokenGeneratorTests
    {
        [Fact]
        public void GenerateToken_ShouldIncludeCorrectClaims_AndBeSigned()
        {
            // Arrange
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c["Jwt:Key"]).Returns("this_is_a_very_secure_and_long_secret_key_12345");

            var generator = new JwtTokenGenerator(mockConfig.Object);
            var user = new Domain.Entities.User("johndoe", "johndoe@example.com", "hashedpass")
            {
                Id = Guid.NewGuid()
            };

            // Act
            var token = generator.GenerateToken(user);
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            // Assert
            Assert.NotNull(token);

            var nameIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "nameid");
            var emailClaim = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email || c.Type == "email");

            Assert.NotNull(nameIdClaim);
            Assert.Equal(user.Id.ToString(), nameIdClaim.Value);

            Assert.NotNull(emailClaim);
            Assert.Equal(user.Email, emailClaim.Value);
        }
    }
}
