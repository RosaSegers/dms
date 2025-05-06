using Organization.Api.Common.Enums;

namespace Organization.Api.Domain.Entities
{
    public class Invite
    {
        public Invite()
        {
        }

        public Invite(Guid organizationId, string email, Guid roleId, Guid invitedById, InviteStatus status)
        {
            Id = Guid.NewGuid();
            OrganizationId = organizationId;
            Email = email;
            RoleId = roleId;
            InvitedById = invitedById;
            Status = status;
            ExpiresAt = DateTime.UtcNow.AddMinutes(30);
        }

        public Invite(Guid id, Guid organizationId, string email, Guid roleId, Guid invitedById, InviteStatus status)
        {
            Id = id;
            OrganizationId = organizationId;
            Email = email;
            RoleId = roleId;
            InvitedById = invitedById;
            Status = status;
            ExpiresAt = DateTime.UtcNow.AddMinutes(30);
        }

        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public string Email { get; set; }
        public Guid RoleId { get; set; }
        public Guid InvitedById { get; set; }
        public InviteStatus Status { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
