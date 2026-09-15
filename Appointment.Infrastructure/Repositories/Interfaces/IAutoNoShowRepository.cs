using Appointment.Domain.DTOs.Settings.Responses;

namespace Appointment.Infrastructure.Repositories.Interfaces;

public interface IAutoNoShowRepository
{
    /// <summary>Run sweep for one org or all orgs (orgId null) for the app.</summary>
    Task<AutoNoShowSweepResultDto> SweepAsync(
        int appId, int? orgId, int batchSize, CancellationToken cancellationToken = default);
}
