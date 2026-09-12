using Appointment.Domain.DTOs.Settings.Requests;
using Appointment.Domain.DTOs.Settings.Responses;

namespace Appointment.Application.Services.Interfaces;

public interface ISettingsService
{
    Task<IReadOnlyList<SettingsMasterSummaryDto>> GetMastersSummaryAsync(
        int orgId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SettingsLookupItemDto>> GetLookupsAsync(
        int orgId, GetLookupsRequest request, CancellationToken cancellationToken = default);

    Task<SettingsLookupItemDto> CreateLookupAsync(
        int orgId, SaveLookupRequest request, CancellationToken cancellationToken = default);

    Task<SettingsLookupItemDto> UpdateLookupAsync(
        int orgId, int lookupId, UpdateLookupRequest request, CancellationToken cancellationToken = default);

    Task DeactivateLookupAsync(
        int orgId, int lookupId, CancellationToken cancellationToken = default);

    Task<AppointmentRulesDto> GetRulesAsync(
        int orgId, CancellationToken cancellationToken = default);

    Task<AppointmentRulesDto> SetRulesAsync(
        int orgId, SaveAppointmentRulesRequest request, CancellationToken cancellationToken = default);
}
