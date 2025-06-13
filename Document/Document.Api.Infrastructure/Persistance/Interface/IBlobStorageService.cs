using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Document.Api.Infrastructure.Persistance.Interface
{
    public interface IBlobStorageService
    {
        Task UploadAsync(Stream fileStream, string blobName, string contentType);
    }
}
