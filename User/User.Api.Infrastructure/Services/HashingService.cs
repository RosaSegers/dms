using BCrypt.Net;
using Microsoft.Extensions.Configuration;
using User.Api.Common.Interfaces;

namespace User.Api.Infrastructure.Services
{
    internal class HashingService(IConfiguration configuration) : IHashingService
    {
        public string Hash(string key)
        {
            return BCrypt.Net.BCrypt.EnhancedHashPassword(key + configuration.GetSection("Security:Peper"));
        }

        public bool Validate(string key, string hash)
        {
            return BCrypt.Net.BCrypt.EnhancedVerify(key + configuration.GetSection("Security:Peper"), hash);
        }
    }
}
