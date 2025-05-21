using System.Security.Claims;
using Auditing.Api.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Auditing.Api.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        //public string UserId => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        public Guid UserId => Guid.NewGuid();
    }
}
