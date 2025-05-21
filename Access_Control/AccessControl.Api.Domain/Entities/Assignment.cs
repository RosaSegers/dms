using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessControl.Api.Domain.Entities
{
    public class Assignment
    {
        public Assignment()
        {
        }

        public Assignment(Guid userId, Guid resourceId, Role role)
        {
            UserId = userId;
            ResourceId = resourceId;
            this.Role = role;
        }

        public Guid UserId { get; set; }
        public Guid ResourceId { get; set; }
        public Role Role{ get; set; }

    }
}
