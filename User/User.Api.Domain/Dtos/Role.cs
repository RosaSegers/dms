using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace User.Api.Domain.Dtos
{
    public class Role
    {
        public string Name { get; set; }
        public List<Permission> Permissions { get; set; }
    }
}
