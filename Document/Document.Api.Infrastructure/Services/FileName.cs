using System.Text;
using Document.Api.Domain.Events;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Document.Api.Infrastructure.Services
{

    public class PolymorphicCosmosSerializer : CosmosSerializer
    {
        private readonly JsonSerializer _serializer;

        public PolymorphicCosmosSerializer()
        {
            _serializer = new JsonSerializer();
        }

        public override T FromStream<T>(Stream stream)
        {
            using (stream)
            using (var sr = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                var jObject = JObject.Load(jsonTextReader);
                var eventType = jObject["EventType"]?.ToString();

                Type targetType = eventType switch
                {
                    nameof(DocumentDeletedEvent) => typeof(DocumentDeletedEvent),
                    nameof(DocumentRolebackEvent) => typeof(DocumentRolebackEvent),
                    nameof(DocumentUploadedEvent) => typeof(DocumentUploadedEvent),
                    nameof(DocumentUpdatedEvent) => typeof(DocumentUpdatedEvent),
                    _ => throw new NotSupportedException($"Unknown event type: {eventType}")
                };

                return (T)jObject.ToObject(targetType, _serializer);
            }
        }

        public override Stream ToStream<T>(T input)
        {
            var streamPayload = new MemoryStream();
            using (var streamWriter = new StreamWriter(streamPayload, Encoding.UTF8, 1024, true))
            using (var jsonWriter = new JsonTextWriter(streamWriter))
            {
                _serializer.Serialize(jsonWriter, input);
                jsonWriter.Flush();
            }
            streamPayload.Position = 0;
            return streamPayload;
        }
    }

}
