using Document.Api.Common.Interfaces;
using Document.Api.Domain.Events;
using Document.Api.Infrastructure.Background.Interfaces;
using Document.Api.Infrastructure.Services.Interface;
using Microsoft.Azure.Cosmos.Linq;
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
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_queue.TryDequeue(out var item))
                {
                    using var scope = _services.CreateScope();

                    var scanner = scope.ServiceProvider.GetRequiredService<IVirusScanner>();
                    var storage = scope.ServiceProvider.GetRequiredService<IDocumentStorage>();
                    var statusService = scope.ServiceProvider.GetRequiredService<IDocumentStatusService>();

                    await statusService.SetStatusAsync(item.Document.DocumentId, "scanning");

                    try
                    {
                        var clean = await scanner.ScanFile(item.File);

                        if (clean)
                        {
                            await storage.AddDocument(item.Document);
                            await statusService.SetStatusAsync(item.Document.DocumentId, "clean");
                        }
                        else
                        {
                            await statusService.SetStatusAsync(item.Document.DocumentId, "malicious");
                            // Optionally: log or store in a rejection list
                        }
                    }
                    catch (Exception ex)
                    {
                        await statusService.SetStatusAsync(item.Document.DocumentId, "error");
                        // Optionally log: Console.WriteLine($"Scan failed: {ex.Message}");
                    }
                }

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
