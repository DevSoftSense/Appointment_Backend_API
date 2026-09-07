using Appointment.Domain.DTOs.Appointments.Requests;
using Appointment.Domain.DTOs.Appointments.Responses;

namespace Appointment.Infrastructure.Repositories.Interfaces;

public interface IAppointmentRepository
{
    Task<IReadOnlyList<AppointmentListItemDto>> GetAppointmentsAsync(
        int orgId,
        int appId,
        GetAppointmentsRequest request,
        CancellationToken cancellationToken = default);

    Task<AppointmentStatsDto> GetAppointmentStatsAsync(
        int orgId,
        int appId,
        long? customerId = null,
        long? professionalId = null,
        CancellationToken cancellationToken = default);

    Task<AppointmentDetailDto?> GetAppointmentByIdAsync(
        int orgId,
        int appId,
        long appointmentId,
        CancellationToken cancellationToken = default);

    Task<AppointmentDetailDto> CreateAppointmentAsync(
        int orgId,
        int appId,
        long createdBy,
        CreateAppointmentRequest request,
        CancellationToken cancellationToken = default);

    Task<AppointmentDetailDto> UpdateAppointmentAsync(
        int orgId,
        int appId,
        long appointmentId,
        long? updatedBy,
        UpdateAppointmentRequest request,
        CancellationToken cancellationToken = default);

    Task<AppointmentDetailDto> CancelAppointmentAsync(
        int orgId,
        int appId,
        long appointmentId,
        long? updatedBy,
        CancelAppointmentRequest request,
        CancellationToken cancellationToken = default);

    Task<AppointmentDetailDto> CheckInAppointmentAsync(
        int orgId,
        int appId,
        long appointmentId,
        long? updatedBy,
        CancellationToken cancellationToken = default);

    Task<AppointmentDetailDto> CompleteAppointmentAsync(
        int orgId,
        int appId,
        long appointmentId,
        long? updatedBy,
        CancellationToken cancellationToken = default);
}
