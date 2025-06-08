using Document.Api.Common.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System;

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
                Console.WriteLine($"Adding document with ID: {document.Id}");
                await _container.CreateItemAsync(document, new PartitionKey(document.Id.ToString()));
                _cache.InvalidateCaches();
                Console.WriteLine($"Successfully added document with ID: {document.Id}");
                return true;
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Error adding document with ID: {document.Id}. Exception: {ex.Message}");
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
                var iterator = _container.GetItemQueryIterator<IDocumentEvent>(query);

                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    results.AddRange(response);
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

                var iterator = _container.GetItemQueryIterator<IDocumentEvent>(query);

                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    results.AddRange(response);
                }

                Console.WriteLine($"Found {results.Count} document(s) with ID: {id}");
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Error fetching document(s) with ID: {id}. Exception: {ex.Message}");
            }

            return results;
        }
    }
}
