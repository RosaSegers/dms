using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Organization.Api.Domain.Entities
{
    public class Member
    {
        public Member() { }

        public Member(Guid organizationId, Guid userId)
        {
            OrganizationId = organizationId;
            UserId = userId;
        }

        public Guid OrganizationId { get; set; }
        public Guid UserId { get; set; }
    }
}
