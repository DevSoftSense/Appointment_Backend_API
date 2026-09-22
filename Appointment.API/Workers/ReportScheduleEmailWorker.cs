using Appointment.Application.Services.Interfaces;

namespace Appointment.API.Workers;

/// <summary>
/// Polls due scheduled report emails every minute, builds CSV, sends via SMTP.
/// </summary>
public sealed class ReportScheduleEmailWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReportScheduleEmailWorker> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

    public ReportScheduleEmailWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<ReportScheduleEmailWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ReportScheduleEmailWorker started (interval {Interval})", _interval);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var schedules = scope.ServiceProvider.GetRequiredService<IReportScheduleService>();
                await schedules.ProcessDueAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ReportScheduleEmailWorker tick failed");
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
