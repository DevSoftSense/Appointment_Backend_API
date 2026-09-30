using Appointment.Domain.DTOs.Referrals.Requests;
using Appointment.Domain.DTOs.Referrals.Responses;

namespace Appointment.Infrastructure.Repositories.Interfaces;

public interface IReferralRepository
{
    Task<ReferralDto?> GetCustomerDefaultAsync(
        int orgId, int appId, long accountId, CancellationToken cancellationToken = default);

    Task<ReferralDto> UpsertCustomerDefaultAsync(
        int orgId, int appId, long accountId, long userId, UpsertReferralRequest request,
        CancellationToken cancellationToken = default);

    Task ClearCustomerDefaultAsync(
        int orgId, int appId, long accountId, long userId, CancellationToken cancellationToken = default);

    Task<ReferralDto?> GetByAppointmentAsync(
        int orgId, int appId, long appointmentId, CancellationToken cancellationToken = default);

    Task<ReferralDto> UpsertForAppointmentAsync(
        int orgId, int appId, long appointmentId, long userId, UpsertReferralRequest request,
        CancellationToken cancellationToken = default);

    Task ClearForAppointmentAsync(
        int orgId, int appId, long appointmentId, long userId, CancellationToken cancellationToken = default);
}
