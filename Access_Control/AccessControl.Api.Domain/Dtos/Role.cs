using AccessControl.Api.Domain.Entities;

namespace AccessControl.Api.Domain.Dtos
{
    public class Role
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<Permission> Permissions { get; set; }
    }
}
