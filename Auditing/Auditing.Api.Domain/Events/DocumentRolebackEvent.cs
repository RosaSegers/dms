using Auditing.Api.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auditing.Api.Domain.Events
{
    public class AuditingRolebackEvent : IAuditingEvent
    {
        public AuditingRolebackEvent(Guid id, float? version, Guid rolledBackByUserId, List<IAuditingEvent> eventsToReapply)
        {
            Id = id;
            Version = version;
            RolledBackByUserId = rolledBackByUserId;
            EventsToReapply = eventsToReapply;
            OccurredAt = DateTime.UtcNow;
        }

        public Guid Id { get; set; }
        public DateTime OccurredAt { get; set; }
        public float? Version { get; set; }
        public Guid RolledBackByUserId { get; set; }
        public List<IAuditingEvent> EventsToReapply { get; set; } = new List<IAuditingEvent>();
    }
}
