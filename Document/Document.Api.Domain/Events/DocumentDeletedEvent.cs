using Document.Api.Common.Interfaces;

namespace Document.Api.Domain.Events
{
    public class DocumentDeletedEvent : IDocumentEvent
    {

        public Guid Id { get; set; }
        public DateTime OccurredAt { get; set; }
        public float? Version { get; set; } = null;
        public Guid DeletedByUserId { get; set; }


        public DocumentDeletedEvent(Guid id, Guid deletedByUserId)
        {
            Id = id;
            OccurredAt = DateTime.UtcNow;
            DeletedByUserId = deletedByUserId;
        }
    }
}
