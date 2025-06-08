using Document.Api.Common.Interfaces;

namespace Document.Api.Domain.Events
{
    public class DocumentDeletedEvent : DocumentEventBase
    {
        public Guid DeletedByUserId { get; set; }


        public DocumentDeletedEvent(Guid id, Guid deletedByUserId)
        {
            this.id = id;
            OccurredAt = DateTime.UtcNow;
            DeletedByUserId = deletedByUserId;
        }
    }
}
