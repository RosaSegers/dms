using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text.Json;

namespace Document.Api.Common.Authorization.Requirements
{
    public class RoleRequirement(string role) : IAuthorizationRequirement
    {
        public string Role { get; } = role ?? throw new ArgumentNullException(nameof(role));
    }

    public class RoleHandler : AuthorizationHandler<RoleRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleRequirement requirement)
        {
            Console.WriteLine(JsonSerializer.Serialize(context.User));
            if (context.User.IsInRole(requirement.Role))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }

    public class RoleAuthorizeAttribute : AuthorizeAttribute
    {
        public RoleAuthorizeAttribute(string Role)
        {
            Policy = Role;
        }
    }
}
