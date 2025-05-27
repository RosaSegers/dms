using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using User.Api.Common.Authorization.Requirements;
using Xunit;

namespace User.Api.Test.Unit
{
    public class PermissionHandlerTests
    {
        [Fact]
        public async Task Should_Succeed_When_User_Has_Permission()
        {
            // Arrange
            var requirement = new RoleRequirement("Admin");
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, "Admin")
            }));

            var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);
            var handler = new RoleHandler();

            // Act
            await handler.HandleAsync(context);

            // Assert
            Assert.True(context.HasSucceeded);
        }

        [Fact]
        public async Task Should_Fail_When_User_Does_Not_Have_Permission()
        {
            // Arrange
            var requirement = new RoleRequirement("User.DELETE");
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("Permission", "User.READ")
            }));

            var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);
            var handler = new RoleHandler();

            // Act
            await handler.HandleAsync(context);

            // Assert
            Assert.False(context.HasSucceeded);
        }

        [Fact]
        public async Task Should_Fail_When_User_Has_No_Claims()
        {
            // Arrange
            var requirement = new RoleRequirement("User.UPDATE");
            var user = new ClaimsPrincipal(new ClaimsIdentity());

            var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);
            var handler = new RoleHandler();

            // Act
            await handler.HandleAsync(context);

            // Assert
            Assert.False(context.HasSucceeded);
        }
    }
}