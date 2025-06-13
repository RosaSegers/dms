using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Document.Api.Infrastructure.Persistance
{
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using Document.Api.Infrastructure.Persistance.Interface;
    using Microsoft.Extensions.Configuration;

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
            var blobClient = _containerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(
                fileStream,
                new BlobHttpHeaders { ContentType = contentType }
            );
        }

        public async Task<Stream> DownloadAsync(string blobName)
        {
            var blobClient = _containerClient.GetBlobClient(blobName);

            if (!await blobClient.ExistsAsync())
                throw new FileNotFoundException($"Blob '{blobName}' not found.");

            var response = await blobClient.DownloadAsync();

            return response.Value.Content;
        }

    }

}
