using Appointment.Domain.DTOs.Payments.Requests;
using Appointment.Domain.DTOs.Payments.Responses;

namespace Appointment.Infrastructure.Repositories.Interfaces;

public interface IPaymentRepository
{
    Task<PaymentEntryListResponse> ListAsync(
        int orgId, int appId, GetPaymentEntriesRequest request,
        CancellationToken cancellationToken = default);

    Task<PaymentEntryStatsDto> ListStatsAsync(
        int orgId, int appId, GetPaymentEntriesRequest request,
        CancellationToken cancellationToken = default);

    Task<AppointmentPaymentDto> GetByAppointmentAsync(
        int orgId, int appId, long appointmentId, CancellationToken cancellationToken = default);

    Task<AppointmentPaymentDto> CreateAsync(
        int orgId, int appId, long appointmentId, long userId, RecordPaymentRequest request,
        CancellationToken cancellationToken = default);

    Task<AppointmentPaymentDto> UpdateBillAsync(
        int orgId, int appId, long appointmentId, long userId, RecordPaymentRequest request,
        CancellationToken cancellationToken = default);

    Task<AppointmentPaymentDto> AddPaymentAsync(
        int orgId, int appId, long appointmentId, long userId, AddPaymentReceiptRequest request,
        CancellationToken cancellationToken = default);

    Task<AppointmentPaymentDto> VoidAsync(
        int orgId, int appId, long appointmentId, long userId,
        CancellationToken cancellationToken = default);
}
