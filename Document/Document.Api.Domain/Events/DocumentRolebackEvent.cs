using Document.Api.Common.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Document.Api.Domain.Events
{
    public class DocumentRolebackEvent : DocumentEventBase
    {
        public DocumentRolebackEvent(Guid id, float? version, Guid rolledBackByUserId, List<IDocumentEvent> eventsToReapply)
        {
            this.id = id;
            Version = version;
            RolledBackByUserId = rolledBackByUserId;
            EventsToReapply = eventsToReapply;
            OccurredAt = DateTime.UtcNow;

        }
        public Guid RolledBackByUserId { get; set; }
        public List<IDocumentEvent> EventsToReapply { get; set; } = new List<IDocumentEvent>();
        [JsonProperty]
        public override string EventType => nameof(DocumentRolebackEvent);
    }
}
