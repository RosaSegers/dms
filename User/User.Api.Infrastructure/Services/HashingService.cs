using BCrypt.Net;
using Microsoft.Extensions.Configuration;
using User.Api.Common.Interfaces;

namespace User.Api.Infrastructure.Services
{
    internal class HashingService() : IHashingService
    {
        public string Hash(string key)
        {
            return BCrypt.Net.BCrypt.EnhancedHashPassword(key);
        }

        public bool Validate(string key, string hash)
        {
            return BCrypt.Net.BCrypt.EnhancedVerify(key, hash);
        }
    }
}
