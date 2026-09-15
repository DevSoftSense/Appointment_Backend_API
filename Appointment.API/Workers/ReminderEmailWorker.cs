using Appointment.Application.Services.Interfaces;

namespace Appointment.API.Workers;

/// <summary>
/// Polls due appointment reminder emails every minute and sends via SMTP.
/// Requires API process to stay running (local SecondConnection / configured Smtp).
/// </summary>
public sealed class ReminderEmailWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReminderEmailWorker> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

    public ReminderEmailWorker(IServiceScopeFactory scopeFactory, ILogger<ReminderEmailWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ReminderEmailWorker started (interval {Interval})", _interval);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var reminders = scope.ServiceProvider.GetRequiredService<IReminderService>();
                await reminders.ProcessDueAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ReminderEmailWorker tick failed");
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
