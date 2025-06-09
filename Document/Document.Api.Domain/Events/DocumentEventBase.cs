using Document.Api.Common.Interfaces;
using Document.Api.Common.Services;
using Newtonsoft.Json;

namespace Document.Api.Domain.Events
{
    public abstract class DocumentEventBase : IDocumentEvent
    {
        [JsonProperty("id")]
        public string Id { get; private set; } = Guid.NewGuid().ToString();
        public Guid DocumentId { get; set; } = Guid.NewGuid();
        public virtual string EventType { get; set; } = string.Empty;
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
        [JsonConverter(typeof(NullableFloatAsStringConverter))]
        public float? Version { get; set; } = null;
    }

}
