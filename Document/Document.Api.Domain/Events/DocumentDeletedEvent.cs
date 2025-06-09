using Document.Api.Common.Interfaces;
using Newtonsoft.Json;

namespace Document.Api.Domain.Events
{
    public class DocumentDeletedEvent : DocumentEventBase
    {
        public Guid DeletedByUserId { get; set; }
        [JsonProperty]
        public override string EventType => nameof(DocumentDeletedEvent);


        public DocumentDeletedEvent(Guid id, Guid deletedByUserId)
        {
            this.DocumentId = id;
            OccurredAt = DateTime.UtcNow;
            DeletedByUserId = deletedByUserId;
        }
    }
}
