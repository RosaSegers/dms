using System.Net.Http.Headers;
using Document.Api.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Document.Api.Infrastructure.Services
{
    public class VirusScanner : IVirusScanner
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private const string VirusTotalApiKey = "22fa922586c549a2d77f12ee36651e23e43e41f31a7ff49807fc7e78f14a9484";
        private const string UploadUrl = "https://www.virustotal.com/api/v3/files";

        public VirusScanner(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<bool> ScanFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            using var content = new MultipartFormDataContent();
            await using var fileStream = file.OpenReadStream();
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "file", file.FileName);

            var request = new HttpRequestMessage(HttpMethod.Post, UploadUrl)
            {
                Content = content
            };
            request.Headers.Add("x-apikey", VirusTotalApiKey);


            var response = await _httpClient.SendAsync(request);
            Console.WriteLine(await response.Content.ReadAsStringAsync());
            if (!response.IsSuccessStatusCode)
                return false;

            return true;
        }
    }
}