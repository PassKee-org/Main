using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PassKee.WorkerServices;

public class DummyLogWorker : BackgroundService
{
    private readonly ILogger<DummyLogWorker> _logger;

    public DummyLogWorker(ILogger<DummyLogWorker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Dummy log worker executing at: {time}", DateTimeOffset.Now);
            // Wait for 1 hour
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}
