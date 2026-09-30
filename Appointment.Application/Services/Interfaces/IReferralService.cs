using Appointment.Domain.DTOs.Referrals.Requests;
using Appointment.Domain.DTOs.Referrals.Responses;

namespace Appointment.Application.Services.Interfaces;

public interface IReferralService
{
    Task<ReferralDto?> GetCustomerDefaultAsync(int orgId, long accountId, CancellationToken cancellationToken = default);

    Task<ReferralDto> UpsertCustomerDefaultAsync(
        int orgId, long accountId, long userId, UpsertReferralRequest request,
        CancellationToken cancellationToken = default);

    Task ClearCustomerDefaultAsync(int orgId, long accountId, long userId, CancellationToken cancellationToken = default);

    Task<ReferralDto?> GetByAppointmentAsync(int orgId, long appointmentId, CancellationToken cancellationToken = default);

    Task<ReferralDto> UpsertForAppointmentAsync(
        int orgId, long appointmentId, long userId, UpsertReferralRequest request,
        CancellationToken cancellationToken = default);

    Task ClearForAppointmentAsync(int orgId, long appointmentId, long userId, CancellationToken cancellationToken = default);
}
