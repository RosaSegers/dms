using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Organization.Api.Domain.Entities
{
    public class Organization
    {
        public Organization()
        {
        }

        public Organization(string name, string slug, Guid ownerId)
        {
            Id = Guid.NewGuid();
            Name = name;
            Slug = slug;
            OwnerId = ownerId;
        }

        public Organization(Guid id, string name, string slug, Guid ownerId)
        {
            Id = id;
            Name = name;
            Slug = slug;
            OwnerId = ownerId;
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public Guid OwnerId { get; set; }
    }
}
