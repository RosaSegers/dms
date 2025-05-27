using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using User.Api.Common.Interfaces;
using User.API.Common.Interfaces.Markers;

namespace User.Api.Domain.Entities
{
    public class User : ICanBeSoftDeleted, IHaveTrackingData
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public User() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public User(string name, string email, string password)
        {
            Id = Guid.NewGuid();
            Name = name;
            Email = email;
            Password = password;
        }

        public User(Guid id, string name, string email, string password)
        {
            Id = id;
            Name = name;
            Email = email;
            Password = password;
        }

        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public int LoginAttempts { get; set; } = 0;
        public DateTime? LastFailedLoginAttempt { get; set; } = DateTime.MinValue;

        public virtual ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();
        public virtual ICollection<PasswordResetToken> PasswordResetTokens { get; private set; } = new List<PasswordResetToken>();

    }
}
