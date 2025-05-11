using Auditing.Api.Common.Interfaces;

namespace Auditing.Api.Domain.Events
{
    public class AuditingDeletedEvent : IAuditingEvent
    {

        public Guid Id { get; set; }
        public DateTime OccurredAt { get; set; }
        public float? Version { get; set; } = null;
        public Guid DeletedByUserId { get; set; }


        public AuditingDeletedEvent(Guid id, Guid deletedByUserId)
        {
            Id = id;
            OccurredAt = DateTime.UtcNow;
            DeletedByUserId = deletedByUserId;
        }
    }
}
