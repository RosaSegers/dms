using Organization.Api.Common.Interfaces.Markers;
using System.ComponentModel.DataAnnotations;

namespace Organization.Api.Domain.Entities
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
            Roles = new List<Role>();
        }

        public User(Guid id, string name, string email, string password)
        {
            Id = id;
            Name = name;
            Email = email;
            Password = password;
            Roles = new List<Role>();
        }

        public User(Guid id, string name, string email, string password, List<Role> roles)
        {
            Id = id;
            Name = name;
            Email = email;
            Password = password;
            Roles = roles;
        }

        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public List<Role> Roles { get; set; } = new List<Role>();

        public void AddRole(Role role)
        {
            Roles.Add(role);
        }

        public void RemoveRole(string role)
        {
            Roles.RemoveAt(Roles.FindIndex(x => x.Name == role));
        }

        public bool HasPermission(string permission)
        {
            return Roles.Any(r => r.HasPermission(permission));
        }


    }
}
