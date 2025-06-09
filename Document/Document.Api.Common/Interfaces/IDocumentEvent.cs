using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Document.Api.Common.Interfaces
{
    public interface IDocumentEvent
    {
        [JsonProperty("id")]
        string Id { get; }
        Guid DocumentId { get; }
        string EventType { get; }
        DateTime OccurredAt { get; }
        float? Version { get; }
    }
}
