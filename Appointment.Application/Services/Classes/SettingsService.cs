using Appointment.Application.Services.Interfaces;
using Appointment.Domain.DTOs.Settings.Requests;
using Appointment.Domain.DTOs.Settings.Responses;
using Appointment.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Appointment.Application.Services.Classes;

public sealed class SettingsService : ISettingsService
{
    private static readonly HashSet<string> AllowedKinds = new(StringComparer.OrdinalIgnoreCase)
    {
        "room_type",
        "appointment_type",
        "appointment_source",
        "cancel_reason"
    };

    private readonly ISettingsRepository _settingsRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SettingsService> _logger;

    public SettingsService(
        ISettingsRepository settingsRepository,
        IConfiguration configuration,
        ILogger<SettingsService> logger)
    {
        _settingsRepository = settingsRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public Task<IReadOnlyList<SettingsMasterSummaryDto>> GetMastersSummaryAsync(
        int orgId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        return _settingsRepository.GetMastersSummaryAsync(orgId, GetAppId(), cancellationToken);
    }

    public Task<IReadOnlyList<SettingsLookupItemDto>> GetLookupsAsync(
        int orgId, GetLookupsRequest request, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        request ??= new GetLookupsRequest();
        ValidateKind(request.LookupKind);
        if (request.Limit <= 0) request.Limit = 100;
        if (request.Offset < 0) request.Offset = 0;
        return _settingsRepository.GetLookupsAsync(orgId, GetAppId(), request, cancellationToken);
    }

    public async Task<SettingsLookupItemDto> CreateLookupAsync(
        int orgId, SaveLookupRequest request, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (request is null) throw new ArgumentNullException(nameof(request));
        ValidateKind(request.LookupKind);
        if (string.IsNullOrWhiteSpace(request.LookupCode))
            throw new ArgumentException("Code is required.");
        if (string.IsNullOrWhiteSpace(request.LookupName))
            throw new ArgumentException("Name is required.");

        try
        {
            return await _settingsRepository.CreateLookupAsync(orgId, GetAppId(), request, cancellationToken);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            _logger.LogWarning(ex, "Duplicate lookup code for org {OrgId}", orgId);
            throw new ArgumentException("This code already exists for the selected master.");
        }
    }

    public async Task<SettingsLookupItemDto> UpdateLookupAsync(
        int orgId, int lookupId, UpdateLookupRequest request, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (lookupId <= 0) throw new ArgumentException("Lookup id is required.");
        if (request is null) throw new ArgumentNullException(nameof(request));

        try
        {
            return await _settingsRepository.UpdateLookupAsync(orgId, GetAppId(), lookupId, request, cancellationToken);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ArgumentException("This code already exists for the selected master.");
        }
    }

    public Task DeactivateLookupAsync(int orgId, int lookupId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (lookupId <= 0) throw new ArgumentException("Lookup id is required.");
        return _settingsRepository.DeactivateLookupAsync(orgId, GetAppId(), lookupId, cancellationToken);
    }

    public Task<AppointmentRulesDto> GetRulesAsync(int orgId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        return _settingsRepository.GetRulesAsync(orgId, GetAppId(), cancellationToken);
    }

    public Task<AppointmentRulesDto> SetRulesAsync(
        int orgId, SaveAppointmentRulesRequest request, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (request is null) throw new ArgumentNullException(nameof(request));
        if (request.AutoNoShowGraceMinutes is < 0 or > 240)
            throw new ArgumentException("Grace minutes must be between 0 and 240.");
        if (!request.AutoNoShowEnabled.HasValue && !request.AutoNoShowGraceMinutes.HasValue)
            throw new ArgumentException("Nothing to save.");

        return _settingsRepository.SetRulesAsync(orgId, GetAppId(), request, cancellationToken);
    }

    private static void ValidateOrg(int orgId)
    {
        if (orgId <= 0)
            throw new ArgumentException("Organisation id is required.", nameof(orgId));
    }

    private static void ValidateKind(string? kind)
    {
        if (string.IsNullOrWhiteSpace(kind) || !AllowedKinds.Contains(kind.Trim()))
            throw new ArgumentException("Valid lookup kind is required (room_type, appointment_type, appointment_source, cancel_reason).");
    }

    private int GetAppId()
    {
        var appId = _configuration.GetValue<int?>("Appointment:AppId")
                    ?? _configuration.GetValue<int?>("Appointment:ProductId")
                    ?? 25;
        return appId;
    }
}
