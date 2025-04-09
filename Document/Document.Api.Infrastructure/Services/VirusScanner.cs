using Document.Api.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Document.Api.Infrastructure.Services
{
    internal class VirusScanner : IVirusScanner
    {
        public async Task<bool> ScanFile(IFormFile file)
        {
            return true;
        }
    }
}
