using AccessControl.Api.Domain.Entities;

namespace AccessControl.Api.Domain.Dtos
{
    public class Role
    {
        public Guid Id { get; set; } = Guid.Empty;
        public string Name { get; set; } = string.Empty;
        public List<Permission> Permissions { get; set; } = new List<Permission>();
    }
}
