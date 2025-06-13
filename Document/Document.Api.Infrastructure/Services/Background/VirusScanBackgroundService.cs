using Document.Api.Common.Interfaces;
using Document.Api.Domain.Events;
using Document.Api.Infrastructure.Background.Interfaces;
using Document.Api.Infrastructure.Services.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Document.Api.Infrastructure.Services.Background
{
    public class VirusScanBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly IDocumentScanQueue _queue;

        public VirusScanBackgroundService(IServiceProvider services, IDocumentScanQueue queue)
        {
            _services = services;
            _queue = queue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("[VirusScanBackgroundService] Started background service.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (_queue.TryDequeue(out var item))
                    {
                        Console.WriteLine($"[VirusScanBackgroundService] Dequeued document {item.Document.DocumentId}");

                        using var scope = _services.CreateScope();

                        var scanner = scope.ServiceProvider.GetRequiredService<IVirusScanner>();
                        var storage = scope.ServiceProvider.GetRequiredService<IDocumentStorage>();
                        var statusService = scope.ServiceProvider.GetRequiredService<IDocumentStatusService>();

                        await statusService.SetStatusAsync(item.Document.DocumentId, "scanning");
                        Console.WriteLine($"[VirusScanBackgroundService] Scanning started for document {item.Document.DocumentId}");

                        var clean = await scanner.ScanFile(item.FileStream, item.FileName, item.ContentType);

                        if (clean)
                        {
                            await storage.AddDocument(item.Document);
                            await statusService.SetStatusAsync(item.Document.DocumentId, "clean");
                            Console.WriteLine($"[VirusScanBackgroundService] Document {item.Document.DocumentId} is clean and stored.");
                        }
                        else
                        {
                            await statusService.SetStatusAsync(item.Document.DocumentId, "malicious");
                            Console.WriteLine($"[VirusScanBackgroundService] Document {item.Document.DocumentId} is malicious.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[VirusScanBackgroundService] Error processing document: {ex.Message}");
                    if (_queue.TryPeek(out var erroredItem)) // optional visibility into which item failed
                    {
                        var statusService = _services.CreateScope().ServiceProvider.GetRequiredService<IDocumentStatusService>();
                        await statusService.SetStatusAsync(erroredItem.Document.DocumentId, "error");
                    }
                }

                await Task.Delay(1000, stoppingToken);
            }

            Console.WriteLine("[VirusScanBackgroundService] Background service stopping.");
        }
    }
}
