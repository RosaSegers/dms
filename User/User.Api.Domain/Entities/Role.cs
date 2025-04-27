using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace User.Api.Domain.Entities
{
    public class Role
    {
        [Key]
        public string Name { get; set; }
        public List<Permission> Permissions { get; set; }
        public virtual ICollection<User> Users { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Role() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public Role(string name, List<Permission> permissions)
        {
            Name = name;
            Permissions = permissions;
        }


        public void AddPermission(Permission permission)
        {
            Permissions.Add(permission);
        }

        public void RemovePermission(string permission) {
            Permissions.RemoveAt(Permissions.FindIndex(p => p.Name == permission)); 
        }

        public bool HasPermission(string permission)
        {
            return Permissions.Any(p => p.Name == permission);
        }
    }
}
