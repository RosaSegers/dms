using System.Security.Cryptography;

namespace User.Api.Domain.Entities
{
    public class PasswordResetToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string Token { get; set; } = RandomNumberGenerator.GetHexString(32);
        public DateTime Expiration { get; set; } = DateTime.UtcNow.AddMinutes(15);
    }
}
