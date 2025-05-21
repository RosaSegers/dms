using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessControl.Api.Domain.Entities
{
    public class Grant
    {
        public Grant()
        {
        }

        public Grant(Guid userId, Guid resourceId, Permission permission)
        {
            UserId = userId;
            ResourceId = resourceId;
            Permission = permission;
        }

        public Guid UserId { get; set; }
        public Guid ResourceId { get; set; }
        public Permission Permission { get; set; }
    }
}
