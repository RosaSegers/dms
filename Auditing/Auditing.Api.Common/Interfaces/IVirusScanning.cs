using Microsoft.AspNetCore.Http;

namespace Auditing.Api.Common.Interfaces
{
    public interface IVirusScanner
    {
        Task<bool> ScanFile(IFormFile file);
    }
}
