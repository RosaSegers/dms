using Document.Api.Common.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

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
            _container = cosmosClient.GetContainer(databaseName, containerName);
        }

        public async Task<bool> AddDocument(IDocumentEvent document)
        {
            try
            {
                await _container.CreateItemAsync(document, new PartitionKey(document.Id.ToString()));
                _cache.InvalidateCaches();
                return true;
            }
            catch (CosmosException ex)
            {
                // Log error appropriately
                return false;
            }
        }

        public async Task<List<IDocumentEvent>> GetDocumentList()
        {
            var query = new QueryDefinition("SELECT * FROM c");
            var iterator = _container.GetItemQueryIterator<IDocumentEvent>(query);
            var results = new List<IDocumentEvent>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response);
            }

            return results;
        }

        public async Task<List<IDocumentEvent>> GetDocumentById(Guid id)
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @id")
                .WithParameter("@id", id.ToString());

            var iterator = _container.GetItemQueryIterator<IDocumentEvent>(query);
            var results = new List<IDocumentEvent>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response);
            }

            return results;
        }
    }
}
