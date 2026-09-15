using Appointment.Application.Services.Interfaces;

namespace Appointment.API.Workers;

/// <summary>
/// Marks overdue scheduled/confirmed appointments as no_show when org auto-no-show is enabled.
/// Requires API process to stay running (same as ReminderEmailWorker).
/// </summary>
public sealed class AutoNoShowWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AutoNoShowWorker> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

    public AutoNoShowWorker(IServiceScopeFactory scopeFactory, ILogger<AutoNoShowWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AutoNoShowWorker started (interval {Interval})", _interval);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<IAutoNoShowService>();
                await service.SweepAsync(orgId: null, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AutoNoShowWorker tick failed");
            }

            try
            {
                await Task.Delay(_interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}
