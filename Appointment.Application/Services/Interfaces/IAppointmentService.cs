using Appointment.Domain.DTOs.Appointments.Requests;
using Appointment.Domain.DTOs.Appointments.Responses;

namespace Appointment.Application.Services.Interfaces;

public interface IAppointmentService
{
    Task<AppointmentListResponse> GetAppointmentsAsync(
        int orgId,
        GetAppointmentsRequest request,
        CancellationToken cancellationToken = default);

    Task<AppointmentStatsDto> GetAppointmentStatsAsync(
        int orgId,
        long? customerId = null,
        long? professionalId = null,
        CancellationToken cancellationToken = default);

    Task<AppointmentDetailDto?> GetAppointmentByIdAsync(
        int orgId,
        long appointmentId,
        CancellationToken cancellationToken = default);

    Task<AppointmentDetailDto> CreateAppointmentAsync(
        int orgId,
        long createdBy,
        CreateAppointmentRequest request,
        CancellationToken cancellationToken = default);

    Task<AppointmentDetailDto> UpdateAppointmentAsync(
        int orgId,
        long appointmentId,
        long updatedBy,
        UpdateAppointmentRequest request,
        CancellationToken cancellationToken = default);

    Task<AppointmentDetailDto> CancelAppointmentAsync(
        int orgId,
        long appointmentId,
        long updatedBy,
        CancelAppointmentRequest request,
        CancellationToken cancellationToken = default);

    Task<AppointmentDetailDto> CheckInAppointmentAsync(
        int orgId,
        long appointmentId,
        long updatedBy,
        CancellationToken cancellationToken = default);

    Task<AppointmentDetailDto> CompleteAppointmentAsync(
        int orgId,
        long appointmentId,
        long updatedBy,
        CancellationToken cancellationToken = default);
}
