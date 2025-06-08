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
                return false;

            // Step 1: Upload file to VirusTotal
            using var content = new MultipartFormDataContent();
            await using var fileStream = file.OpenReadStream();
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "file", file.FileName);

            var uploadRequest = new HttpRequestMessage(HttpMethod.Post, UploadUrl)
            {
                Content = content
            };
            uploadRequest.Headers.Add("X-Apikey", _apiKey);

            var uploadResponse = await _httpClient.SendAsync(uploadRequest);
            if (!uploadResponse.IsSuccessStatusCode)
                return false;

            var uploadJson = await uploadResponse.Content.ReadAsStringAsync();
            var uploadDoc = JsonDocument.Parse(uploadJson);
            var analysisId = uploadDoc.RootElement
                .GetProperty("data")
                .GetProperty("id")
                .GetString();

            if (string.IsNullOrEmpty(analysisId))
                return false;

            // Step 2: Poll the analysis endpoint
            string analysisUrl = string.Format(AnalysisUrlTemplate, analysisId);
            int maxRetries = 5;
            int delayBetweenRetriesMs = 2000;

            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                await Task.Delay(delayBetweenRetriesMs);

                var analysisRequest = new HttpRequestMessage(HttpMethod.Get, analysisUrl);
                analysisRequest.Headers.Add("X-Apikey", _apiKey);

                var analysisResponse = await _httpClient.SendAsync(analysisRequest);
                if (!analysisResponse.IsSuccessStatusCode)
                    continue;

                var analysisJson = await analysisResponse.Content.ReadAsStringAsync();
                var analysisDoc = JsonDocument.Parse(analysisJson);
                var root = analysisDoc.RootElement;

                string status = root
                    .GetProperty("data")
                    .GetProperty("attributes")
                    .GetProperty("status")
                    .GetString() ?? "not_completed";

                if (status == "completed")
                {
                    var stats = root
                        .GetProperty("data")
                        .GetProperty("attributes")
                        .GetProperty("stats");

                    int malicious = stats.GetProperty("malicious").GetInt32();

                    return malicious == 0;
                }
            }

            // Scan not completed or malicious
            return false;
        }
    }
}
