using Appointment.Domain.DTOs.Payments.Requests;
using Appointment.Domain.DTOs.Payments.Responses;

namespace Appointment.Application.Services.Interfaces;

public interface IPaymentService
{
    Task<PaymentEntryListResponse> ListAsync(
        int orgId, GetPaymentEntriesRequest request, CancellationToken cancellationToken = default);

    Task<PaymentEntryStatsDto> ListStatsAsync(
        int orgId, GetPaymentEntriesRequest request, CancellationToken cancellationToken = default);

    Task<AppointmentPaymentDto> GetByAppointmentAsync(
        int orgId, long appointmentId, CancellationToken cancellationToken = default);

    Task<AppointmentPaymentDto> CreateAsync(
        int orgId, long appointmentId, long userId, RecordPaymentRequest request,
        CancellationToken cancellationToken = default);

    Task<AppointmentPaymentDto> UpdateBillAsync(
        int orgId, long appointmentId, long userId, RecordPaymentRequest request,
        CancellationToken cancellationToken = default);

    Task<AppointmentPaymentDto> AddPaymentAsync(
        int orgId, long appointmentId, long userId, AddPaymentReceiptRequest request,
        CancellationToken cancellationToken = default);

    Task<AppointmentPaymentDto> VoidAsync(
        int orgId, long appointmentId, long userId,
        CancellationToken cancellationToken = default);
}
