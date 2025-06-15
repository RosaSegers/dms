using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Document.Api.Infrastructure.Persistance
{
    using System.Runtime.InteropServices;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using Document.Api.Infrastructure.Persistance.Interface;
    using Microsoft.Azure.Cosmos.Linq;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Identity.Client;

    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobContainerClient _containerClient;

        public BlobStorageService(IConfiguration config)
        {
            var connectionString = config.GetConnectionString("BlobStorage");
            var containerName = config["BlobContainerName"];
            _containerClient = new BlobContainerClient(connectionString, containerName);
            _containerClient.CreateIfNotExists(PublicAccessType.None);
        }

        public async Task UploadAsync(Stream fileStream, string blobName, string contentType)
        {
            Console.WriteLine($"[BlobStorageService] Uploading blob with name: {blobName}");

            var blobClient = _containerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(
                fileStream,
                new BlobHttpHeaders { ContentType = contentType }
            );

            foreach (var blob in _containerClient.GetBlobs(prefix: blobName.Split("_")[0]))
            {
                if (blob.Name.Equals(blobName))
                    continue;

                var blobToArchive = _containerClient.GetBlobClient(blob.Name);
                if (blobToArchive.GetProperties().Value.AccessTier == AccessTier.Cold)
                    continue;

                Console.WriteLine($"[BlobStorageService] Archiving blob {blob.Name}: {blob.Properties.LastModified}");
                await blobToArchive.SetAccessTierAsync(AccessTier.Cold);
            }
        }

        public async Task<Stream> DownloadAsync(string blobName)
        {
            var blobClient = _containerClient.GetBlobClient(blobName);

            if (!await blobClient.ExistsAsync())
                throw new FileNotFoundException($"Blob '{blobName}' not found.");

            var response = await blobClient.DownloadAsync();

            return response.Value.Content;
        }

        public async Task DeleteAsync(string blobName)
        {
            var blobClient = _containerClient.GetBlobClient($"{blobName}");

            if (!await blobClient.ExistsAsync())
                throw new FileNotFoundException($"Blob '{blobName}' could not be found.");

            await blobClient.DeleteAsync();
        }

        public async Task DeletePrefixAsync(string blobName)
        {
            var blobClients = _containerClient.GetBlobsAsync(prefix: $"{blobName}");

            if (blobClients.IsNull())
                throw new FileNotFoundException($"Blob '{blobName}' could not be found.");

            await foreach (var blobItem in blobClients)
            {
                var blob = _containerClient.GetBlobClient(blobName);

                try
                {
                    await blob.DeleteAsync();
                    Console.WriteLine($"Blob '{blob.Name}' has successfully been deleted.");
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Blob '{blob.Name}' was not deleted due to an exeption by the name of: {ex.Message}");
                }
            }


        }
    }

}
