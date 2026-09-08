using Appointment.Domain.DTOs.Appointments.Responses;
using Microsoft.AspNetCore.Http;

namespace Appointment.Application.Services.Interfaces;

public interface IAppointmentDocumentService
{
    Task<IReadOnlyList<AppointmentDocumentDto>> ListAsync(
        int orgId, long appointmentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AppointmentDocumentDto>> ListByCustomerAsync(
        int orgId, long customerId, CancellationToken cancellationToken = default);

    Task<AppointmentDocumentDto> UploadAsync(
        int orgId,
        long uploadedBy,
        long appointmentId,
        IFormFile file,
        string? documentType = null,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        int orgId, long appointmentId, long documentId, CancellationToken cancellationToken = default);
}
