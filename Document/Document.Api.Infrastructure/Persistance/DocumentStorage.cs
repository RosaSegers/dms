using Document.Api.Common.Interfaces;
using Document.Api.Domain.Events;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Document.Api.Infrastructure.Persistance
{
    public class DocumentStorage : IDocumentStorage
    {
        private readonly Container _container;
        private readonly ICacheService _cache;

        public DocumentStorage(ICacheService cache, IConfiguration configuration, CosmosClient cosmosClient)
        {
            _cache = cache;

            var databaseName = configuration["CosmosDb:DatabaseName"];
            var containerName = configuration["CosmosDb:ContainerName"];
            Console.WriteLine($"Initializing Cosmos DB container with Database: {databaseName}, Container: {containerName}");
            _container = cosmosClient.GetContainer(databaseName, containerName);
        }

        public async Task<bool> AddDocument(IDocumentEvent document)
        {
            try
            {
                Console.WriteLine($"Adding document with ID: {document.id}");

                // Serialize explicitly to JSON string
                var json = JsonConvert.SerializeObject(document);
                var doc = JsonConvert.DeserializeObject<JObject>(json);

                Console.WriteLine($"Payload being sent to Cosmos:\n{doc}");
                Console.WriteLine($"Serialized JSON contains id: {doc["id"]?.ToString()}");
                await _container.CreateItemAsync(doc, new PartitionKey(document.id.ToString()));
                _cache.InvalidateCaches();
                Console.WriteLine($"Successfully added document with ID: {document.id}");
                return true;
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Error adding document with ID: {document.id}. Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<List<IDocumentEvent>> GetDocumentList()
        {
            var results = new List<IDocumentEvent>();

            try
            {
                Console.WriteLine("Fetching all documents from container.");
                var query = new QueryDefinition("SELECT * FROM c");
                var iterator = _container.GetItemQueryIterator<JObject>(query);

                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();

                    foreach (var item in response)
                    {
                        var documentEvent = DeserializeDocumentEvent(item);
                        if (documentEvent != null)
                            results.Add(documentEvent);
                    }
                }

                Console.WriteLine($"Fetched {results.Count} documents.");
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Error fetching document list. Exception: {ex.Message}");
            }

            return results;
        }

        public async Task<List<IDocumentEvent>> GetDocumentById(Guid id)
        {
            var results = new List<IDocumentEvent>();

            try
            {
                Console.WriteLine($"Fetching document(s) with ID: {id}");
                var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @id")
                    .WithParameter("@id", id.ToString());

                var iterator = _container.GetItemQueryIterator<JObject>(query);

                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();

                    foreach (var item in response)
                    {
                        var documentEvent = DeserializeDocumentEvent(item);
                        if (documentEvent != null)
                            results.Add(documentEvent);
                    }
                }

                Console.WriteLine($"Found {results.Count} document(s) with ID: {id}");
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Error fetching document(s) with ID: {id}. Exception: {ex.Message}");
            }

            return results;
        }

        private IDocumentEvent? DeserializeDocumentEvent(JObject json)
        {
            // Assumes your documents have an "EventType" property
            var eventType = json["EventType"]?.Value<string>();

            return eventType switch
            {
                nameof(DocumentDeletedEvent) => json.ToObject<DocumentDeletedEvent>(),
                nameof(DocumentRolebackEvent) => json.ToObject<DocumentRolebackEvent>(),
                nameof(DocumentUploadedEvent) => json.ToObject<DocumentUploadedEvent>(),
                nameof(DocumentUpdatedEvent) => json.ToObject<DocumentUpdatedEvent>(),
                _ => null
            };
        }
    }
}
