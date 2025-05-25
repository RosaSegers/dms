using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace User.Api.Domain.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; set; } = Guid.Empty;
        public Guid UserId { get; set; } = Guid.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(30); 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRevoked { get; set; } = false;
        public bool IsUsed { get; set; } = false;
    }
}
