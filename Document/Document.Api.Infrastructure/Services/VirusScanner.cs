using System.Net.Http.Headers;
using Document.Api.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Document.Api.Infrastructure.Services
{
    public class VirusScanner : IVirusScanner
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string UploadUrl = "https://www.virustotal.com/api/v3/files";
        private const string AnalysisUrlTemplate = "https://www.virustotal.com/api/v3/analyses/{0}";

        public VirusScanner(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["VirusTotal"] ?? throw new Exception("VirusTotal API key not found in configuration.");
        }

        public async Task<bool> ScanFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Invalid file");

            // Step 1: Upload file
            using var content = new MultipartFormDataContent();
            await using var stream = file.OpenReadStream();
            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(streamContent, "file", file.FileName);

            var uploadRequest = new HttpRequestMessage(HttpMethod.Post, UploadUrl)
            {
                Content = content
            };
            uploadRequest.Headers.Add("x-apikey", _apiKey);

            var uploadResponse = await _httpClient.SendAsync(uploadRequest);
            if (!uploadResponse.IsSuccessStatusCode)
            {
                // Optional: log uploadResponse.StatusCode
                return false;
            }

            var uploadJson = await uploadResponse.Content.ReadAsStringAsync();
            var uploadDoc = JsonDocument.Parse(uploadJson);
            var analysisId = uploadDoc.RootElement
                .GetProperty("data")
                .GetProperty("id")
                .GetString();

            if (string.IsNullOrWhiteSpace(analysisId))
                return false;

            // Step 2: Poll for result
            string analysisUrl = string.Format(AnalysisUrlTemplate, analysisId);
            int retries = 10;
            int delay = 3000;

            for (int i = 0; i < retries; i++)
            {
                await Task.Delay(delay);

                var analysisRequest = new HttpRequestMessage(HttpMethod.Get, analysisUrl);
                analysisRequest.Headers.Add("x-apikey", _apiKey);

                var analysisResponse = await _httpClient.SendAsync(analysisRequest);
                if (!analysisResponse.IsSuccessStatusCode)
                    continue;

                var analysisJson = await analysisResponse.Content.ReadAsStringAsync();
                var analysisDoc = JsonDocument.Parse(analysisJson);
                var root = analysisDoc.RootElement;

                string status = root.GetProperty("data").GetProperty("attributes").GetProperty("status").GetString();
                if (status != "completed")
                    continue;

                var stats = root.GetProperty("data").GetProperty("attributes").GetProperty("stats");
                int malicious = stats.GetProperty("malicious").GetInt32();
                int suspicious = stats.GetProperty("suspicious").GetInt32();
                int undetected = stats.GetProperty("undetected").GetInt32();

                // Optional logging
                Console.WriteLine($"Scan Result: Malicious={malicious}, Suspicious={suspicious}, Undetected={undetected}");

                // File is safe only if zero malicious/suspicious flags
                return malicious == 0 && suspicious == 0;
            }

            // Timeout or error during polling
            return false;
        }
    }
}
