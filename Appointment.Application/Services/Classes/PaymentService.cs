using Appointment.Application.Services.Interfaces;
using Appointment.Domain.DTOs.Payments.Requests;
using Appointment.Domain.DTOs.Payments.Responses;
using Appointment.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Appointment.Application.Services.Classes;

public sealed class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _repository;
    private readonly IConfiguration _configuration;

    public PaymentService(IPaymentRepository repository, IConfiguration configuration)
    {
        _repository = repository;
        _configuration = configuration;
    }

    public Task<PaymentEntryListResponse> ListAsync(
        int orgId, GetPaymentEntriesRequest request, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        request ??= new GetPaymentEntriesRequest();
        if (request.Limit <= 0) request.Limit = 50;
        if (request.Limit > 200) request.Limit = 200;
        if (request.Offset < 0) request.Offset = 0;
        if (!string.IsNullOrWhiteSpace(request.PaymentStatus))
        {
            var s = request.PaymentStatus.Trim().ToLowerInvariant();
            if (s is not "paid" and not "partial" and not "unpaid" and not "to_collect")
                throw new ArgumentException("Payment status must be paid, partial, unpaid, or to_collect.");
            request.PaymentStatus = s;
        }
        return _repository.ListAsync(orgId, GetAppId(), request, cancellationToken);
    }

    public Task<PaymentEntryStatsDto> ListStatsAsync(
        int orgId, GetPaymentEntriesRequest request, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        request ??= new GetPaymentEntriesRequest();
        return _repository.ListStatsAsync(orgId, GetAppId(), request, cancellationToken);
    }

    public Task<AppointmentPaymentDto> GetByAppointmentAsync(
        int orgId, long appointmentId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (appointmentId <= 0) throw new ArgumentException("Appointment id is required.");
        return _repository.GetByAppointmentAsync(orgId, GetAppId(), appointmentId, cancellationToken);
    }

    public Task<AppointmentPaymentDto> CreateAsync(
        int orgId, long appointmentId, long userId, RecordPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (appointmentId <= 0) throw new ArgumentException("Appointment id is required.");
        if (userId <= 0) throw new ArgumentException("User id is required.");
        ValidateBillRequest(request);
        return _repository.CreateAsync(orgId, GetAppId(), appointmentId, userId, request, cancellationToken);
    }

    public Task<AppointmentPaymentDto> UpdateBillAsync(
        int orgId, long appointmentId, long userId, RecordPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (appointmentId <= 0) throw new ArgumentException("Appointment id is required.");
        if (userId <= 0) throw new ArgumentException("User id is required.");
        ValidateBillRequest(request, requireReceived: false);
        return _repository.UpdateBillAsync(orgId, GetAppId(), appointmentId, userId, request, cancellationToken);
    }

    public Task<AppointmentPaymentDto> AddPaymentAsync(
        int orgId, long appointmentId, long userId, AddPaymentReceiptRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (appointmentId <= 0) throw new ArgumentException("Appointment id is required.");
        if (userId <= 0) throw new ArgumentException("User id is required.");
        if (request is null) throw new ArgumentNullException(nameof(request));
        if (request.AmountReceived <= 0)
            throw new ArgumentException("Amount received must be greater than zero.");
        request.PaymentMethod = NormalizeMethod(request.PaymentMethod);
        return _repository.AddPaymentAsync(orgId, GetAppId(), appointmentId, userId, request, cancellationToken);
    }

    public Task<AppointmentPaymentDto> VoidAsync(
        int orgId, long appointmentId, long userId,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (appointmentId <= 0) throw new ArgumentException("Appointment id is required.");
        if (userId <= 0) throw new ArgumentException("User id is required.");
        return _repository.VoidAsync(orgId, GetAppId(), appointmentId, userId, cancellationToken);
    }

    private static void ValidateBillRequest(RecordPaymentRequest request, bool requireReceived = true)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));

        var mode = (request.GstMode ?? "none").Trim().ToLowerInvariant();
        if (mode is not "none" and not "cgst_sgst" and not "igst")
            throw new ArgumentException("GST mode must be none, cgst_sgst, or igst.");
        request.GstMode = mode;

        if (request.GstPercent < 0 || request.GstPercent > 100)
            throw new ArgumentException("GST percent must be between 0 and 100.");

        if (request.Lines is null || request.Lines.Count == 0)
            throw new ArgumentException("At least one bill line is required.");

        foreach (var line in request.Lines)
        {
            if (string.IsNullOrWhiteSpace(line.Description))
                throw new ArgumentException("Each line needs a description.");
            if (line.Amount <= 0)
                throw new ArgumentException("Each line amount must be greater than zero.");
            line.Description = line.Description.Trim();
        }

        if (request.AmountReceived < 0)
            throw new ArgumentException("Amount received cannot be negative.");

        request.PaymentMethod = NormalizeMethod(request.PaymentMethod);
        _ = requireReceived;
    }

    private static string NormalizeMethod(string? method)
    {
        var m = (method ?? "cash").Trim().ToLowerInvariant();
        return m is "cash" or "card" or "upi" or "other" ? m : "other";
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
