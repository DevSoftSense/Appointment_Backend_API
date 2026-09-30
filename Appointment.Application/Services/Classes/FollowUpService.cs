using Appointment.Application.Helpers;
using Appointment.Application.Services.Interfaces;
using Appointment.Domain.DTOs.Appointments.Responses;
using Appointment.Domain.DTOs.FollowUps.Requests;
using Appointment.Domain.DTOs.FollowUps.Responses;
using Appointment.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Appointment.Application.Services.Classes;

public sealed class FollowUpService : IFollowUpService
{
    private readonly IFollowUpRepository _repository;
    private readonly IReminderService _reminderService;
    private readonly ISettingsService _settingsService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<FollowUpService> _logger;

    public FollowUpService(
        IFollowUpRepository repository,
        IReminderService reminderService,
        ISettingsService settingsService,
        IConfiguration configuration,
        ILogger<FollowUpService> logger)
    {
        _repository = repository;
        _reminderService = reminderService;
        _settingsService = settingsService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<IReadOnlyList<FollowUpListItemDto>> ListByParentAsync(
        int orgId, long parentAppointmentId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (parentAppointmentId <= 0)
            throw new ArgumentException("Parent appointment id is required.");

        return await _repository.ListByParentAsync(orgId, GetAppId(), parentAppointmentId, cancellationToken);
    }

    public async Task<AppointmentDetailDto> CreateOneAsync(
        int orgId, long parentAppointmentId, long createdBy,
        CreateFollowUpRequest request, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (parentAppointmentId <= 0)
            throw new ArgumentException("Parent appointment id is required.");
        if (createdBy <= 0)
            throw new ArgumentException("Created-by user is required.");
        if (request is null)
            throw new ArgumentNullException(nameof(request));
        if (request.StartDatetime == default)
            throw new ArgumentException("Start date/time is required.");
        if (request.StartDatetime < DateTimeOffset.UtcNow)
            throw new ArgumentException("Cannot book a follow-up in the past. Choose a future time.");

        await EnsureInBookingWindowAsync(orgId, request.StartDatetime, cancellationToken);

        _logger.LogInformation(
            "Creating follow-up from parent={ParentId} org={OrgId}", parentAppointmentId, orgId);

        var created = await _repository.CreateOneAsync(
            orgId, GetAppId(), parentAppointmentId, createdBy, request, cancellationToken);

        await _reminderService.SyncForAppointmentAsync(orgId, created.AppointmentId, createdBy, cancellationToken);
        return created;
    }

    public async Task<FollowUpSeriesPreviewDto> PreviewSeriesAsync(
        int orgId, long parentAppointmentId,
        CreateFollowUpSeriesRequest request, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (parentAppointmentId <= 0)
            throw new ArgumentException("Parent appointment id is required.");
        ValidateSeriesRequest(request);
        await EnsureInBookingWindowAsync(orgId, request.FirstStartDatetime, cancellationToken);
        if (string.Equals(request.Mode, "range", StringComparison.OrdinalIgnoreCase) && request.EndsOn.HasValue)
        {
            var rules = await _settingsService.GetRulesAsync(orgId, cancellationToken);
            BookingWindowHelper.EnsureDateInWindow(
                request.EndsOn.Value,
                BookingWindowHelper.ClampMonths(rules.BookingWindowMonths));
        }

        return await _repository.PreviewSeriesAsync(
            orgId, GetAppId(), parentAppointmentId, request, cancellationToken);
    }

    public async Task<FollowUpSeriesResultDto> CreateSeriesAsync(
        int orgId, long parentAppointmentId, long createdBy,
        CreateFollowUpSeriesRequest request, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (parentAppointmentId <= 0)
            throw new ArgumentException("Parent appointment id is required.");
        if (createdBy <= 0)
            throw new ArgumentException("Created-by user is required.");
        ValidateSeriesRequest(request);
        await EnsureInBookingWindowAsync(orgId, request.FirstStartDatetime, cancellationToken);
        if (string.Equals(request.Mode, "range", StringComparison.OrdinalIgnoreCase) && request.EndsOn.HasValue)
        {
            var rules = await _settingsService.GetRulesAsync(orgId, cancellationToken);
            BookingWindowHelper.EnsureDateInWindow(
                request.EndsOn.Value,
                BookingWindowHelper.ClampMonths(rules.BookingWindowMonths));
        }

        _logger.LogInformation(
            "Creating follow-up series from parent={ParentId} org={OrgId} mode={Mode}",
            parentAppointmentId, orgId, request.Mode);

        var result = await _repository.CreateSeriesAsync(
            orgId, GetAppId(), parentAppointmentId, createdBy, request, cancellationToken);

        if (result.Created is not null)
        {
            foreach (var child in result.Created)
            {
                await _reminderService.SyncForAppointmentAsync(
                    orgId, child.AppointmentId, createdBy, cancellationToken);
            }
        }

        return result;
    }

    private async Task EnsureInBookingWindowAsync(
        int orgId, DateTimeOffset start, CancellationToken cancellationToken)
    {
        var apptDate = BookingWindowHelper.ResolveAppointmentDate(null, start);
        var rules = await _settingsService.GetRulesAsync(orgId, cancellationToken);
        BookingWindowHelper.EnsureDateInWindow(
            apptDate,
            BookingWindowHelper.ClampMonths(rules.BookingWindowMonths));
    }

    private static void ValidateSeriesRequest(CreateFollowUpSeriesRequest request)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));
        if (request.FirstStartDatetime == default)
            throw new ArgumentException("First start date/time is required.");
        if (request.FirstStartDatetime < DateTimeOffset.UtcNow)
            throw new ArgumentException("Cannot book a follow-up series starting in the past.");

        var mode = (request.Mode ?? "count").Trim().ToLowerInvariant();
        if (mode is not ("count" or "range"))
            throw new ArgumentException("Mode must be count or range.");
        request.Mode = mode;

        var unit = (request.IntervalUnit ?? "weeks").Trim().ToLowerInvariant();
        if (unit is not ("days" or "weeks"))
            throw new ArgumentException("Interval unit must be days or weeks.");
        request.IntervalUnit = unit;

        if (request.IntervalValue < 1 || request.IntervalValue > 52)
            throw new ArgumentException("Interval value must be between 1 and 52.");

        if (mode == "count")
        {
            if (request.Count is null || request.Count < 1 || request.Count > 52)
                throw new ArgumentException("Count must be between 1 and 52.");
        }
        else if (request.EndsOn is null)
        {
            throw new ArgumentException("End date is required for range mode.");
        }
    }

    private static void ValidateOrg(int orgId)
    {
        if (orgId <= 0)
            throw new ArgumentException("Organization is required.");
    }

    private int GetAppId()
    {
        var appId = _configuration.GetValue<int?>("Appointment:AppId")
                    ?? _configuration.GetValue<int?>("Appointment:ProductId")
                    ?? 25;
        return appId;
    }
}
