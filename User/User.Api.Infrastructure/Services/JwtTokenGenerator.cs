using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using User.Api.Domain.Entities;
using User.Api.Infrastructure.Persistance;

namespace User.Api.Infrastructure.Services
{
    public class JwtTokenGenerator(IConfiguration config) : IJwtTokenGenerator
    {
        public string GenerateToken(Domain.Entities.User user)
        {
            var key = Encoding.UTF8.GetBytes(config["Jwt:Key"] ?? "");
            var tokenHandler = new JwtSecurityTokenHandler();

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, "User")
            };

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(descriptor);
            return tokenHandler.WriteToken(token);
        }
    }

    public class RefreshTokenGenerator(UserDatabaseContext _context) : IRefreshTokenGenerator
    {
        public string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }

        public async Task<string> GenerateAndStoreRefreshTokenAsync(Domain.Entities.User user)
        {
            var token = GenerateRefreshToken();

            var existingTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == user.Id && !rt.IsRevoked)
                .ToListAsync();

            foreach (var t in existingTokens)
                t.IsRevoked = true;

            _context.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.Id,
                Token = token,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
            });

            await _context.SaveChangesAsync();

            return token;
        }


        public async Task RevokeOldRefreshTokenAsync(Domain.Entities.User user)
        {
            var oldTokens = await _context.RefreshTokens.Where(rt => rt.UserId == user.Id && rt.IsRevoked != true).ToListAsync();

            foreach (var oldToken in oldTokens)
            {
                oldToken.IsRevoked = true;
            }

            await _context.SaveChangesAsync();
        }
    }

    public interface IJwtTokenGenerator
    {
        string GenerateToken(Domain.Entities.User user);
    }

    public interface IRefreshTokenGenerator
    {
        string GenerateRefreshToken();

        Task<string> GenerateAndStoreRefreshTokenAsync(Domain.Entities.User user);

        Task RevokeOldRefreshTokenAsync(Domain.Entities.User user);
    }
}
