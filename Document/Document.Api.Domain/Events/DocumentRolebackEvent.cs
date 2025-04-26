using Document.Api.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Document.Api.Domain.Events
{
    public class DocumentRolebackEvent : IDocumentEvent
    {
        public Guid Id { get; set; }
        public DateTime OccurredAt { get; set; }
        public float? Version { get; set; }
        public Guid RolledBackByUserId { get; set; }
        public List<IDocumentEvent> EventsToReapply { get; set; } = new List<IDocumentEvent>();
    }
}
