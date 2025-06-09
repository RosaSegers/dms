using Document.Api.Common.Interfaces;
using Document.Api.Domain.Events;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
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
                Console.WriteLine($"Adding document with ID: '{document.DocumentId}'");

                if (document.DocumentId == Guid.Empty)
                {
                    Console.WriteLine("Warning: Document ID is Guid.Empty. This might cause issues with partition key or uniqueness.");
                }

                var partitionKeyValue = document.DocumentId.ToString();
                var partitionKey = new PartitionKey(partitionKeyValue);

                string json = JsonConvert.SerializeObject(document); // check if it throws
                Console.WriteLine($"Serialized document: {json}");
                var response = await _container.CreateItemAsync<object>(document, partitionKey);

                Console.WriteLine($"Successfully added document with ID: {document.DocumentId}");
                Console.WriteLine($"Request Charge: {response.RequestCharge}");
                Console.WriteLine($"Activity Id: {response.ActivityId}");
                Console.WriteLine($"Status Code: {response.StatusCode}");

                _cache.InvalidateCaches();
                return true;
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Cosmos DB error adding document with ID: {document.DocumentId}");
                Console.WriteLine($"StatusCode: {ex.StatusCode}");
                Console.WriteLine($"SubStatusCode: {ex.SubStatusCode}");
                Console.WriteLine($"ActivityId: {ex.ActivityId}");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"RequestCharge: {ex.RequestCharge}");
                Console.WriteLine($"RetryAfter: {ex.RetryAfter}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error adding document with ID: {document.DocumentId}");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
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
            catch (Exception ex)
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching document(s) with ID: {id}. Exception: {ex.Message}");
            }

            return results;
        }

        private IDocumentEvent? DeserializeDocumentEvent(JObject json)
        {
            var eventType = json["EventType"]?.Value<string>();

            if (string.IsNullOrEmpty(eventType))
            {
                Console.WriteLine("Document missing 'EventType' field, skipping deserialization.");
                return null;
            }

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
