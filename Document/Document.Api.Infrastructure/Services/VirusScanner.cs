using System.Net.Http.Headers;
using Document.Api.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;

namespace Document.Api.Infrastructure.Services
{
    public class VirusScanner : IVirusScanner
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string UploadUrl = "https://www.virustotal.com/api/v3/files";
        private const string AnalysisUrlTemplate = "https://www.virustotal.com/api/v3/files/{0}";

        public VirusScanner(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["VirusTotal"] ?? throw new Exception("VirusTotal API key not found in configuration.");
        }

        public async Task<bool> ScanFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Invalid file");

            await using var stream = file.OpenReadStream();

            var fileHash = await ComputeSHA256Async(stream);
            string? analysisId = await GetAnalysisIdByHash(fileHash);

            if (string.IsNullOrWhiteSpace(analysisId))
            {
                analysisId = await UploadFileToVirusTotal(stream, file.FileName, file.ContentType);
                if (string.IsNullOrWhiteSpace(analysisId))
                    return false;
            }

            return await CheckAnalysisResult(analysisId);
        }

        public async Task<bool> ScanFile(Stream fileStream, string fileName, string contentType)
        {
            if (fileStream == null || fileStream.Length == 0)
                throw new ArgumentException("Invalid stream");

            var fileHash = await ComputeSHA256Async(fileStream);
            string? analysisId = await GetAnalysisIdByHash(fileHash);

            if (string.IsNullOrWhiteSpace(analysisId))
            {
                analysisId = await UploadFileToVirusTotal(fileStream, fileName, contentType);
                if (string.IsNullOrWhiteSpace(analysisId))
                    return false;
            }

            return await CheckAnalysisResult(analysisId);
        }


        private async Task<string?> UploadFileToVirusTotal(Stream file, string fileName, string contentType)
        {
            using var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(file);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            content.Add(streamContent, "file", fileName);

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

        private async Task<string?> GetAnalysisIdByHash(string hash)
        {
            var requestUrl = $"https://www.virustotal.com/api/v3/files/{hash}";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Add("x-apikey", _apiKey);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null; // File not found, safe to upload
                }

                Console.WriteLine($"[VirusScanner] Hash check failed: {response.StatusCode}");
                return null;
            }

            using var jsonDoc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return jsonDoc.RootElement.GetProperty("data").GetProperty("id").GetString();
        }

        private async Task<bool> CheckAnalysisResult(string analysisId)
        {
            string analysisUrl = string.Format(AnalysisUrlTemplate, analysisId);
            int delay = 3000; // Wait 3 seconds between checks

            while (true)
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

                var attributes = root.GetProperty("data").GetProperty("attributes");

                JsonElement statsElement;
                if (!attributes.TryGetProperty("last_analysis_stats", out statsElement) &&
                    !attributes.TryGetProperty("stats", out statsElement))
                {
                    Console.WriteLine("[VirusScanner] No stats available in response.");
                    return false; // or handle as appropriate
                }

                int malicious = statsElement.GetProperty("malicious").GetInt32();
                int suspicious = statsElement.GetProperty("suspicious").GetInt32();

                Console.WriteLine($"Scan Result: Malicious={malicious}, Suspicious={suspicious}");

                return malicious == 0 && suspicious == 0;
            }
        }

        private static async Task<string> ComputeSHA256Async(Stream file)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = await sha256.ComputeHashAsync(file);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }
}
