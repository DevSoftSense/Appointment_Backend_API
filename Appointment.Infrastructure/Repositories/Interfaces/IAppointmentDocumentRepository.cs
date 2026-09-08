using Appointment.Domain.DTOs.Appointments.Responses;

namespace Appointment.Infrastructure.Repositories.Interfaces;

public interface IAppointmentDocumentRepository
{
    Task<IReadOnlyList<AppointmentDocumentDto>> ListAsync(
        int orgId, int appId, long appointmentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AppointmentDocumentDto>> ListByCustomerAsync(
        int orgId, int appId, long customerId, CancellationToken cancellationToken = default);

    Task<AppointmentDocumentDto?> GetByIdAsync(
        int orgId, int appId, long documentId, CancellationToken cancellationToken = default);

    Task<AppointmentDocumentDto> CreateAsync(
        int orgId,
        int appId,
        long appointmentId,
        long uploadedBy,
        string documentName,
        string filePath,
        string? documentType,
        int? fiscalYearId = null,
        CancellationToken cancellationToken = default);

    Task<(bool Deleted, string? FilePath)> DeleteAsync(
        int orgId,
        int appId,
        long appointmentId,
        long documentId,
        CancellationToken cancellationToken = default);
}
