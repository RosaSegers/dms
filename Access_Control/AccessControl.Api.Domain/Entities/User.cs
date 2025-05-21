using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessControl.Api.Domain.Entities
{
    public abstract class User
    {
        public Guid Id { get; set; }
    }
}
