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

            string? analysisId = await UploadFileToVirusTotal(file);
            if (string.IsNullOrWhiteSpace(analysisId))
                return false;

            return await CheckAnalysisResult(analysisId);
        }

        private async Task<string?> UploadFileToVirusTotal(IFormFile file)
        {
            using var content = new MultipartFormDataContent();
            await using var stream = file.OpenReadStream();
            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(streamContent, "file", file.FileName);

            var request = new HttpRequestMessage(HttpMethod.Post, UploadUrl)
            {
                Content = content
            };
            request.Headers.Add("x-apikey", _apiKey);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[VirusScanner] Upload failed with status: {response.StatusCode}");
                return null;
            }

            using var jsonDoc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return jsonDoc.RootElement
                          .GetProperty("data")
                          .GetProperty("id")
                          .GetString();
        }

        private async Task<bool> CheckAnalysisResult(string analysisId)
        {
            string analysisUrl = string.Format(AnalysisUrlTemplate, analysisId);
            int retries = 15;
            int delay = 3000;

            for (int i = 0; i < retries; i++)
            {
                await Task.Delay(delay);

                var request = new HttpRequestMessage(HttpMethod.Get, analysisUrl);
                request.Headers.Add("x-apikey", _apiKey);

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[VirusScanner] Failed to fetch analysis: {response.StatusCode}");
                    continue;
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                string status = root.GetProperty("data").GetProperty("attributes").GetProperty("status").GetString();
                if (status != "completed")
                    continue;

                var stats = root.GetProperty("data").GetProperty("attributes").GetProperty("stats");
                int malicious = stats.GetProperty("malicious").GetInt32();
                int suspicious = stats.GetProperty("suspicious").GetInt32();

                Console.WriteLine($"[VirusScanner] Scan complete: Malicious={malicious}, Suspicious={suspicious}");

                return malicious == 0 && suspicious == 0;
            }

            Console.WriteLine("[VirusScanner] Analysis timed out or incomplete after retries.");
            return false;
        }
    }
}
