using Organization.Api.Common.Enums;

namespace Organization.Api.Domain.Dtos
{
    public class Invite
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public string Email { get; set; }
        public Guid RoleId { get; set; }
        public Guid InvitedById { get; set; }
        public InviteStatus Status { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
