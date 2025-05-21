using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Document.Api.Common.Interfaces
{
    public interface IVirusScanner
    {
        Task<bool> ScanFile(IFormFile file);
    }
}
