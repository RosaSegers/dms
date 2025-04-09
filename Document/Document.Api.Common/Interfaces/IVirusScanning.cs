using Microsoft.AspNetCore.Http;

namespace Document.Api.Common.Interfaces
{
    public interface IVirusScanner
    {
        Task<bool> ScanFile(IFormFile file);
    }
}
