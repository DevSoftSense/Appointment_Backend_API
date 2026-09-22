using Appointment.Application.Helpers;
using Appointment.Application.Services.Interfaces;
using Appointment.Domain.DTOs.Appointments.Requests;
using Appointment.Domain.DTOs.Appointments.Responses;
using Appointment.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Appointment.Application.Services.Classes;

public sealed class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IReminderService _reminderService;
    private readonly ISettingsService _settingsService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentService(
        IAppointmentRepository appointmentRepository,
        IReminderService reminderService,
        ISettingsService settingsService,
        IConfiguration configuration,
        ILogger<AppointmentService> logger)
    {
        _appointmentRepository = appointmentRepository;
        _reminderService = reminderService;
        _settingsService = settingsService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AppointmentListResponse> GetAppointmentsAsync(
        int orgId,
        GetAppointmentsRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        request ??= new GetAppointmentsRequest();
        if (request.Limit <= 0) request.Limit = 50;
        if (request.Offset < 0) request.Offset = 0;

        var appId = GetAppId();
        var items = await _appointmentRepository.GetAppointmentsAsync(orgId, appId, request, cancellationToken);

        return new AppointmentListResponse
        {
            Items = items,
            TotalCount = items.Count > 0 ? items[0].TotalCount : 0
        };
    }

    public async Task<AppointmentStatsDto> GetAppointmentStatsAsync(
        int orgId,
        long? customerId = null,
        long? professionalId = null,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        var appId = GetAppId();
        return await _appointmentRepository.GetAppointmentStatsAsync(
            orgId, appId, customerId, professionalId, cancellationToken);
    }

    public async Task<AppointmentDetailDto?> GetAppointmentByIdAsync(
        int orgId,
        long appointmentId,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (appointmentId <= 0)
            throw new ArgumentException("Appointment id is required.", nameof(appointmentId));

        var appId = GetAppId();
        return await _appointmentRepository.GetAppointmentByIdAsync(orgId, appId, appointmentId, cancellationToken);
    }

    public async Task<AppointmentDetailDto> CreateAppointmentAsync(
        int orgId,
        long createdBy,
        CreateAppointmentRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (request is null)
            throw new ArgumentNullException(nameof(request));
        if (createdBy <= 0)
            throw new ArgumentException("Created-by user is required.");
        if (request.CustomerId <= 0)
            throw new ArgumentException("Customer is required.");
        if (request.ProfessionalId <= 0)
            throw new ArgumentException("Professional is required.");
        if (request.ProductId <= 0)
            throw new ArgumentException("Service is required.");
        if (request.StartDatetime == default)
            throw new ArgumentException("Start date/time is required.");
        if (request.StartDatetime < DateTimeOffset.UtcNow)
            throw new ArgumentException("Cannot book an appointment in the past. Choose a future time.");
        if (request.EndDatetime.HasValue && request.EndDatetime.Value <= request.StartDatetime)
            throw new ArgumentException("End time must be after start time.");

        var apptDate = BookingWindowHelper.ResolveAppointmentDate(
            request.AppointmentDate, request.StartDatetime);
        var rules = await _settingsService.GetRulesAsync(orgId, cancellationToken);
        BookingWindowHelper.EnsureDateInWindow(
            apptDate,
            BookingWindowHelper.ClampMonths(rules.BookingWindowMonths));

        var appId = GetAppId();
        _logger.LogInformation(
            "Creating appointment org={OrgId} customer={CustomerId} professional={ProfessionalId} product={ProductId}",
            orgId, request.CustomerId, request.ProfessionalId, request.ProductId);

        var created = await _appointmentRepository.CreateAppointmentAsync(
            orgId, appId, createdBy, request, cancellationToken);

        await _reminderService.SyncForAppointmentAsync(orgId, created.AppointmentId, createdBy, cancellationToken);
        return created;
    }

    public async Task<AppointmentDetailDto> UpdateAppointmentAsync(
        int orgId,
        long appointmentId,
        long updatedBy,
        UpdateAppointmentRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (appointmentId <= 0)
            throw new ArgumentException("Appointment id is required.", nameof(appointmentId));
        if (request is null)
            throw new ArgumentNullException(nameof(request));
        if (request.StartDatetime.HasValue
            && request.EndDatetime.HasValue
            && request.EndDatetime.Value <= request.StartDatetime.Value)
            throw new ArgumentException("End time must be after start time.");
        // Past starts are blocked in SQL when the start actually changes; keep-window edits of an
        // already-past booking are allowed so notes/status can still be updated.

        var appId = GetAppId();
        var existing = await _appointmentRepository.GetAppointmentByIdAsync(
            orgId, appId, appointmentId, cancellationToken);
        if (existing is null)
            throw new ArgumentException("Appointment not found.");

        // If staff changes the calendar date, new date must be inside advance booking window.
        // Keeping the original date (even if now outside the window) is allowed.
        if (request.StartDatetime.HasValue || request.AppointmentDate.HasValue)
        {
            var newDate = BookingWindowHelper.ResolveAppointmentDate(
                request.AppointmentDate,
                request.StartDatetime ?? existing.StartDatetime);
            var oldDate = BookingWindowHelper.ResolveAppointmentDate(
                existing.AppointmentDate,
                existing.StartDatetime);
            if (newDate != oldDate)
            {
                var rules = await _settingsService.GetRulesAsync(orgId, cancellationToken);
                BookingWindowHelper.EnsureDateInWindow(
                    newDate,
                    BookingWindowHelper.ClampMonths(rules.BookingWindowMonths));
            }
        }

        var updated = await _appointmentRepository.UpdateAppointmentAsync(
            orgId, appId, appointmentId, updatedBy > 0 ? updatedBy : null, request, cancellationToken);

        await _reminderService.SyncForAppointmentAsync(orgId, updated.AppointmentId, updatedBy, cancellationToken);
        return updated;
    }

    public async Task<AppointmentDetailDto> CancelAppointmentAsync(
        int orgId,
        long appointmentId,
        long updatedBy,
        CancelAppointmentRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (appointmentId <= 0)
            throw new ArgumentException("Appointment id is required.", nameof(appointmentId));

        request ??= new CancelAppointmentRequest();
        var appId = GetAppId();
        var cancelled = await _appointmentRepository.CancelAppointmentAsync(
            orgId, appId, appointmentId, updatedBy > 0 ? updatedBy : null, request, cancellationToken);

        await _reminderService.CancelForAppointmentAsync(orgId, appointmentId, cancellationToken);
        return cancelled;
    }

    public async Task<AppointmentDetailDto> CheckInAppointmentAsync(
        int orgId,
        long appointmentId,
        long updatedBy,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (appointmentId <= 0)
            throw new ArgumentException("Appointment id is required.", nameof(appointmentId));

        var appId = GetAppId();
        return await _appointmentRepository.CheckInAppointmentAsync(
            orgId, appId, appointmentId, updatedBy > 0 ? updatedBy : null, cancellationToken);
    }

    public async Task<AppointmentDetailDto> CompleteAppointmentAsync(
        int orgId,
        long appointmentId,
        long updatedBy,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (appointmentId <= 0)
            throw new ArgumentException("Appointment id is required.", nameof(appointmentId));

        var appId = GetAppId();
        return await _appointmentRepository.CompleteAppointmentAsync(
            orgId, appId, appointmentId, updatedBy > 0 ? updatedBy : null, cancellationToken);
    }

    public async Task<AppointmentDetailDto> MarkNoShowAsync(
        int orgId,
        long appointmentId,
        long updatedBy,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (appointmentId <= 0)
            throw new ArgumentException("Appointment id is required.", nameof(appointmentId));

        var appId = GetAppId();
        var updated = await _appointmentRepository.MarkNoShowAsync(
            orgId, appId, appointmentId, updatedBy > 0 ? updatedBy : null, cancellationToken);

        await _reminderService.CancelForAppointmentAsync(orgId, appointmentId, cancellationToken);
        return updated;
    }

    private int GetAppId()
    {
        var appId = _configuration.GetValue<int?>("Appointment:AppId")
                    ?? _configuration.GetValue<int?>("Appointment:ProductId")
                    ?? 0;
        if (appId <= 0)
            throw new InvalidOperationException("Appointment:AppId (or ProductId) is not configured.");
        return appId;
    }

    private static void ValidateOrg(int orgId)
    {
        if (orgId <= 0)
            throw new ArgumentException("Organisation ID is required.");
    }
}
