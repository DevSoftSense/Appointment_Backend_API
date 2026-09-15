using Appointment.Domain.DTOs.Settings.Responses;

namespace Appointment.Application.Services.Interfaces;

public interface IAutoNoShowService
{
    /// <summary>Worker: all orgs for app. Manual smoke: pass orgId.</summary>
    Task<AutoNoShowSweepResultDto> SweepAsync(int? orgId = null, CancellationToken cancellationToken = default);
}
