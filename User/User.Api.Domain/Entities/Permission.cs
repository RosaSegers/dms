using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace User.Api.Domain.Entities
{
    public class Permission
    {
        [Key]
        public string Name { get; set; }
        public string Description { get; set; }
        public virtual ICollection<Role> Roles {  get; set; }


#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Permission() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public Permission(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}
