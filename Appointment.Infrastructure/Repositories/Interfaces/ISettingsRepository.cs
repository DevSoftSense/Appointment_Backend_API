using Appointment.Domain.DTOs.Settings.Requests;
using Appointment.Domain.DTOs.Settings.Responses;

namespace Appointment.Infrastructure.Repositories.Interfaces;

public interface ISettingsRepository
{
    Task<IReadOnlyList<SettingsMasterSummaryDto>> GetMastersSummaryAsync(
        int orgId, int appId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SettingsLookupItemDto>> GetLookupsAsync(
        int orgId, int appId, GetLookupsRequest request, CancellationToken cancellationToken = default);

    Task<SettingsLookupItemDto> CreateLookupAsync(
        int orgId, int appId, SaveLookupRequest request, CancellationToken cancellationToken = default);

    Task<SettingsLookupItemDto> UpdateLookupAsync(
        int orgId, int appId, int lookupId, UpdateLookupRequest request, CancellationToken cancellationToken = default);

    Task DeactivateLookupAsync(
        int orgId, int appId, int lookupId, CancellationToken cancellationToken = default);

    Task<AppointmentRulesDto> GetRulesAsync(
        int orgId, int appId, CancellationToken cancellationToken = default);

    Task<AppointmentRulesDto> SetRulesAsync(
        int orgId, int appId, SaveAppointmentRulesRequest request, CancellationToken cancellationToken = default);
}
