using Appointment.Application.Services.Interfaces;
using Appointment.Domain.DTOs.Settings.Responses;
using Appointment.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Appointment.Application.Services.Classes;

public sealed class AutoNoShowService : IAutoNoShowService
{
    private readonly IAutoNoShowRepository _repository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AutoNoShowService> _logger;

    public AutoNoShowService(
        IAutoNoShowRepository repository,
        IConfiguration configuration,
        ILogger<AutoNoShowService> logger)
    {
        _repository = repository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AutoNoShowSweepResultDto> SweepAsync(
        int? orgId = null, CancellationToken cancellationToken = default)
    {
        var result = await _repository.SweepAsync(GetAppId(), orgId, batchSize: 100, cancellationToken);
        if (result.MarkedCount > 0)
        {
            _logger.LogInformation(
                "Auto no-show sweep marked {Count} appointment(s); cancelled {Reminders} reminder(s)",
                result.MarkedCount, result.RemindersCancelled);
        }

        return result;
    }

    private int GetAppId()
    {
        return _configuration.GetValue<int?>("Appointment:AppId")
               ?? _configuration.GetValue<int?>("Appointment:ProductId")
               ?? 25;
    }
}
