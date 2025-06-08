using Document.Api.Common.Interfaces;

namespace Document.Api.Domain.Events
{
    public abstract class DocumentEventBase : IDocumentEvent
    {
        public Guid id { get; set; } = Guid.NewGuid();
        public virtual string EventType { get; set; } = string.Empty;
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
        public float? Version { get; set; } = null;
    }

}
