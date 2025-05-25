using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Auditing.Api.Common.Authorization.Requirements
{
    public class RoleRequirement(string role) : IAuthorizationRequirement
    {
        public string Role { get; } = role ?? throw new ArgumentNullException(nameof(role));
    }

    public class RoleHandler : AuthorizationHandler<RoleRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleRequirement requirement)
        {
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
