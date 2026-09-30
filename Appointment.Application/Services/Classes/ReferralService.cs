using Appointment.Application.Services.Interfaces;
using Appointment.Domain.DTOs.Referrals.Requests;
using Appointment.Domain.DTOs.Referrals.Responses;
using Appointment.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Appointment.Application.Services.Classes;

public sealed class ReferralService : IReferralService
{
    private readonly IReferralRepository _repository;
    private readonly IConfiguration _configuration;

    public ReferralService(IReferralRepository repository, IConfiguration configuration)
    {
        _repository = repository;
        _configuration = configuration;
    }

    public Task<ReferralDto?> GetCustomerDefaultAsync(
        int orgId, long accountId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (accountId <= 0) throw new ArgumentException("Customer id is required.");
        return _repository.GetCustomerDefaultAsync(orgId, GetAppId(), accountId, cancellationToken);
    }

    public Task<ReferralDto> UpsertCustomerDefaultAsync(
        int orgId, long accountId, long userId, UpsertReferralRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (accountId <= 0) throw new ArgumentException("Customer id is required.");
        if (userId <= 0) throw new ArgumentException("User id is required.");
        ValidateUpsert(request);
        return _repository.UpsertCustomerDefaultAsync(
            orgId, GetAppId(), accountId, userId, request, cancellationToken);
    }

    public Task ClearCustomerDefaultAsync(
        int orgId, long accountId, long userId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (accountId <= 0) throw new ArgumentException("Customer id is required.");
        return _repository.ClearCustomerDefaultAsync(orgId, GetAppId(), accountId, userId, cancellationToken);
    }

    public Task<ReferralDto?> GetByAppointmentAsync(
        int orgId, long appointmentId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (appointmentId <= 0) throw new ArgumentException("Appointment id is required.");
        return _repository.GetByAppointmentAsync(orgId, GetAppId(), appointmentId, cancellationToken);
    }

    public Task<ReferralDto> UpsertForAppointmentAsync(
        int orgId, long appointmentId, long userId, UpsertReferralRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (appointmentId <= 0) throw new ArgumentException("Appointment id is required.");
        if (userId <= 0) throw new ArgumentException("User id is required.");
        ValidateUpsert(request);
        return _repository.UpsertForAppointmentAsync(
            orgId, GetAppId(), appointmentId, userId, request, cancellationToken);
    }

    public Task ClearForAppointmentAsync(
        int orgId, long appointmentId, long userId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (appointmentId <= 0) throw new ArgumentException("Appointment id is required.");
        return _repository.ClearForAppointmentAsync(
            orgId, GetAppId(), appointmentId, userId, cancellationToken);
    }

    private static void ValidateUpsert(UpsertReferralRequest request)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        var type = (request.ReferredByType ?? "").Trim().ToLowerInvariant();
        if (type is not "professional" and not "customer" and not "external" and not "other")
            throw new ArgumentException("Referral type must be professional, customer, external, or other.");

        request.ReferredByType = type;

        if (type == "professional" && !(request.ReferredByEmployeeId > 0))
            throw new ArgumentException("Select the referring professional.");
        if (type == "customer" && !(request.ReferredByAccountId > 0))
            throw new ArgumentException("Select the referring customer.");
        if (type == "external" && string.IsNullOrWhiteSpace(request.ReferredByName))
            throw new ArgumentException("Enter the external referrer name.");
    }

    private int GetAppId()
    {
        var appId = _configuration.GetValue<int?>("Appointment:AppId")
                    ?? _configuration.GetValue<int?>("Appointment:ProductId")
                    ?? 0;
        if (appId <= 0)
            throw new InvalidOperationException("Appointment:AppId (or ProductId) is not configured.");
        return appId;
    }

    private static void ValidateOrg(int orgId)
    {
        if (orgId <= 0)
            throw new ArgumentException("Organisation ID is required.");
    }
}
