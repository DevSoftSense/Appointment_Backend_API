using Appointment.Domain.DTOs.Appointments.Responses;
using Appointment.Domain.DTOs.FollowUps.Requests;
using Appointment.Domain.DTOs.FollowUps.Responses;

namespace Appointment.Application.Services.Interfaces;

public interface IFollowUpService
{
    Task<IReadOnlyList<FollowUpListItemDto>> ListByParentAsync(
        int orgId, long parentAppointmentId, CancellationToken cancellationToken = default);

    Task<AppointmentDetailDto> CreateOneAsync(
        int orgId, long parentAppointmentId, long createdBy,
        CreateFollowUpRequest request, CancellationToken cancellationToken = default);

    Task<FollowUpSeriesPreviewDto> PreviewSeriesAsync(
        int orgId, long parentAppointmentId,
        CreateFollowUpSeriesRequest request, CancellationToken cancellationToken = default);

    Task<FollowUpSeriesResultDto> CreateSeriesAsync(
        int orgId, long parentAppointmentId, long createdBy,
        CreateFollowUpSeriesRequest request, CancellationToken cancellationToken = default);
}
