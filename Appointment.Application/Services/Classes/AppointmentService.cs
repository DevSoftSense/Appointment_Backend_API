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
    private readonly IConfiguration _configuration;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentService(
        IAppointmentRepository appointmentRepository,
        IConfiguration configuration,
        ILogger<AppointmentService> logger)
    {
        _appointmentRepository = appointmentRepository;
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
        if (request.EndDatetime.HasValue && request.EndDatetime.Value <= request.StartDatetime)
            throw new ArgumentException("End time must be after start time.");

        var appId = GetAppId();
        _logger.LogInformation(
            "Creating appointment org={OrgId} customer={CustomerId} professional={ProfessionalId} product={ProductId}",
            orgId, request.CustomerId, request.ProfessionalId, request.ProductId);

        return await _appointmentRepository.CreateAppointmentAsync(
            orgId, appId, createdBy, request, cancellationToken);
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

        var appId = GetAppId();
        return await _appointmentRepository.UpdateAppointmentAsync(
            orgId, appId, appointmentId, updatedBy > 0 ? updatedBy : null, request, cancellationToken);
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
        return await _appointmentRepository.CancelAppointmentAsync(
            orgId, appId, appointmentId, updatedBy > 0 ? updatedBy : null, request, cancellationToken);
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
