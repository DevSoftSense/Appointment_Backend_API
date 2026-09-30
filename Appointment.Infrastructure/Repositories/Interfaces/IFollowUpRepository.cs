using Appointment.Domain.DTOs.Appointments.Responses;
using Appointment.Domain.DTOs.FollowUps.Requests;
using Appointment.Domain.DTOs.FollowUps.Responses;

namespace Appointment.Infrastructure.Repositories.Interfaces;

public interface IFollowUpRepository
{
    Task<IReadOnlyList<FollowUpListItemDto>> ListByParentAsync(
        int orgId, int appId, long parentAppointmentId, CancellationToken cancellationToken = default);

    Task<AppointmentDetailDto> CreateOneAsync(
        int orgId, int appId, long parentAppointmentId, long createdBy,
        CreateFollowUpRequest request, CancellationToken cancellationToken = default);

    Task<FollowUpSeriesPreviewDto> PreviewSeriesAsync(
        int orgId, int appId, long parentAppointmentId,
        CreateFollowUpSeriesRequest request, CancellationToken cancellationToken = default);

    Task<FollowUpSeriesResultDto> CreateSeriesAsync(
        int orgId, int appId, long parentAppointmentId, long createdBy,
        CreateFollowUpSeriesRequest request, CancellationToken cancellationToken = default);
}
