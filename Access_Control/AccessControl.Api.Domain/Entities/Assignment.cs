using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessControl.Api.Domain.Entities
{
    public class Assignment
    {
        public Assignment(Guid userId, Guid resourceId, Guid roleId)
        {
            UserId = userId;
            ResourceId = resourceId;
            RoleId = roleId;
        }

        public Guid UserId { get; set; }
        public Guid ResourceId { get; set; }
        public Guid RoleId { get; set; }

    }
}
