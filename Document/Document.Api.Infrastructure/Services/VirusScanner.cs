using System.Net.Http.Headers;
using Document.Api.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Security.Cryptography;

namespace Document.Api.Infrastructure.Services
{
    public class VirusScanner(HttpClient httpClient, IConfiguration config) : IVirusScanner
    {
        private readonly string _apiKey = config["VirusTotal"] ?? throw new Exception("VirusTotal API key not found in configuration.");
        private const string UploadUrl = "https://www.virustotal.com/api/v3/files";
        private const string AnalysisUrlTemplate = "https://www.virustotal.com/api/v3/files/{0}";

        public async Task<bool> ScanFile(IFormFile file)
        {
            Console.WriteLine("[VirusScanner] Starting scan for IFormFile.");

            if (file == null || file.Length == 0)
                throw new ArgumentException("Invalid file");

            await using var stream = file.OpenReadStream();
            Console.WriteLine($"[VirusScanner] File length: {file.Length}");

            if (stream.CanSeek)
                stream.Position = 0;

            Console.WriteLine("[VirusScanner] Computing SHA256 hash...");
            var fileHash = await ComputeSHA256Async(stream);
            Console.WriteLine($"[VirusScanner] SHA256 hash: {fileHash}");

            if (stream.CanSeek)
                stream.Position = 0;

            var analysisId = await GetAnalysisIdByHash(fileHash);

            if (string.IsNullOrWhiteSpace(analysisId))
            {
                Console.WriteLine("[VirusScanner] No existing hash found. Uploading file...");
                analysisId = await UploadFileToVirusTotal(stream, file.FileName, file.ContentType);
                if (string.IsNullOrWhiteSpace(analysisId))
                {
                    Console.WriteLine("[VirusScanner] Upload failed or no analysis ID returned.");
                    return false;
                }
            }

            return await CheckAnalysisResult(analysisId);
        }

        public async Task<bool> ScanFile(Stream fileStream, string fileName, string contentType)
        {
            Console.WriteLine("[VirusScanner] Starting scan for raw Stream.");

            if (fileStream == null)
                throw new ArgumentException("Invalid stream");

            if (fileStream.CanSeek)
            {
                fileStream.Position = 0;
                Console.WriteLine($"[VirusScanner] Stream length: {fileStream.Length}, Position: {fileStream.Position}");
            }

            if (fileStream.Length == 0)
                throw new ArgumentException("Invalid stream");

            Console.WriteLine("[VirusScanner] Computing SHA256 hash...");
            var fileHash = await ComputeSHA256Async(fileStream);
            Console.WriteLine($"[VirusScanner] SHA256 hash: {fileHash}");

            if (fileStream.CanSeek)
                fileStream.Position = 0;

            var analysisId = await GetAnalysisIdByHash(fileHash);

            if (string.IsNullOrWhiteSpace(analysisId))
            {
                Console.WriteLine("[VirusScanner] No existing hash found. Uploading file...");
                analysisId = await UploadFileToVirusTotal(fileStream, fileName, contentType);
                if (string.IsNullOrWhiteSpace(analysisId))
                {
                    Console.WriteLine("[VirusScanner] Upload failed or no analysis ID returned.");
                    return false;
                }
            }

            return await CheckAnalysisResult(analysisId);
        }

        private async Task<string?> UploadFileToVirusTotal(Stream file, string fileName, string contentType)
        {
            Console.WriteLine("[VirusScanner] Uploading file to VirusTotal...");

            using var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(file);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            content.Add(streamContent, "file", fileName);

            var request = new HttpRequestMessage(HttpMethod.Post, UploadUrl)
            {
                Content = content
            };
            request.Headers.Add("x-apikey", _apiKey);

            var response = await httpClient.SendAsync(request);
            Console.WriteLine($"[VirusScanner] Upload response status: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[VirusScanner] Upload failed: {await response.Content.ReadAsStringAsync()}");
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(json);
            var id = jsonDoc.RootElement.GetProperty("data").GetProperty("id").GetString();
            Console.WriteLine($"[VirusScanner] Upload successful. Analysis ID: {id}");
            return id;
        }

        private async Task<string?> GetAnalysisIdByHash(string hash)
        {
            Console.WriteLine($"[VirusScanner] Checking hash on VirusTotal: {hash}");

            var requestUrl = $"https://www.virustotal.com/api/v3/files/{hash}";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Add("x-apikey", _apiKey);

            var response = await httpClient.SendAsync(request);
            Console.WriteLine($"[VirusScanner] Hash check status: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Console.WriteLine("[VirusScanner] File hash not found. Proceeding to upload.");
                    return null;
                }

                Console.WriteLine($"[VirusScanner] Hash check failed: {await response.Content.ReadAsStringAsync()}");
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(json);
            var id = jsonDoc.RootElement.GetProperty("data").GetProperty("id").GetString();
            Console.WriteLine($"[VirusScanner] Existing analysis ID found: {id}");
            return id;
        }

        private async Task<bool> CheckAnalysisResult(string analysisId)
        {
            Console.WriteLine($"[VirusScanner] Checking analysis result for ID: {analysisId}");
            string analysisUrl = string.Format(AnalysisUrlTemplate, analysisId);
            int delay = 3000;

            while (true)
            {
                await Task.Delay(delay);

                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, analysisUrl);
                    request.Headers.Add("x-apikey", _apiKey);

                    var response = await httpClient.SendAsync(request);
                    Console.WriteLine($"[VirusScanner] Analysis check status: {response.StatusCode}");

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"[VirusScanner] Analysis request failed: {await response.Content.ReadAsStringAsync()}");
                        continue;
                    }

                    var analysisJson = await response.Content.ReadAsStringAsync();
                    using var analysisDoc = JsonDocument.Parse(analysisJson);
                    var root = analysisDoc.RootElement;

                    if (!root.TryGetProperty("data", out JsonElement dataElement) ||
                        !dataElement.TryGetProperty("attributes", out JsonElement attributes))
                    {
                        Console.WriteLine("[VirusScanner] Invalid response: Missing 'data' or 'attributes'");
                        return false;
                    }

                    string? status = null;
                    if (attributes.TryGetProperty("status", out JsonElement statusElement))
                    {
                        status = statusElement.GetString();
                        Console.WriteLine($"[VirusScanner] Analysis status: {status}");
                    }
                    else
                    {
                        Console.WriteLine("[VirusScanner] No status field. Assuming completed.");
                    }

                    if (status != null && status != "completed")
                    {
                        Console.WriteLine("[VirusScanner] Analysis still in progress. Retrying...");
                        continue;
                    }

                    JsonElement statsElement;
                    if (!attributes.TryGetProperty("last_analysis_stats", out statsElement) &&
                        !attributes.TryGetProperty("stats", out statsElement))
                    {
                        Console.WriteLine("[VirusScanner] No stats found.");
                        return false;
                    }

                    int malicious = 0, suspicious = 0;
                    if (statsElement.TryGetProperty("malicious", out var malEl) && malEl.TryGetInt32(out var malVal))
                        malicious = malVal;
                    else
                        Console.WriteLine("[VirusScanner] 'malicious' count missing or invalid.");

                    if (statsElement.TryGetProperty("suspicious", out var suspEl) && suspEl.TryGetInt32(out var suspVal))
                        suspicious = suspVal;
                    else
                        Console.WriteLine("[VirusScanner] 'suspicious' count missing or invalid.");

                    Console.WriteLine($"[VirusScanner] Final Scan Results: Malicious={malicious}, Suspicious={suspicious}");

                    return malicious == 0 && suspicious == 0;
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"[VirusScanner] JSON error: {ex.Message}");
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[VirusScanner] Unexpected error: {ex.Message}");
                    return false;
                }
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
