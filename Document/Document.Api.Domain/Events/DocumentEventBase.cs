using Document.Api.Common.Interfaces;
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
        [JsonIgnore]
        public int? Version { get; set; } = null;
        [JsonProperty("Version")]
        private string? VersionString => Version?.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
    }

}
