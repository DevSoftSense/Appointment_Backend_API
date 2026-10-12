using Appointment.Application.Services.Interfaces;

namespace Appointment.API.Workers;

/// <summary>
/// Sends appointment reminder emails at each row's scheduled_at (not on a coarse 1-minute poll).
/// Requires API process to stay running with SMTP configured.
/// </summary>
public sealed class ReminderEmailWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReminderEmailWorker> _logger;

    public ReminderEmailWorker(IServiceScopeFactory scopeFactory, ILogger<ReminderEmailWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ReminderEmailWorker started (precise scheduled_at timing)");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var reminders = scope.ServiceProvider.GetRequiredService<IReminderService>();
                await reminders.RunWorkerTickAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ReminderEmailWorker tick failed");
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }
}
