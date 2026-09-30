using System.Text.Json;
using Appointment.Domain.DTOs.Referrals.Requests;
using Appointment.Domain.DTOs.Referrals.Responses;
using Appointment.Infrastructure.Data;
using Appointment.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Appointment.Infrastructure.Repositories.Classes;

/// <summary>
/// Calls appointment.fn_appointment_referral(p_action, …). No inline table SQL.
/// </summary>
public sealed class ReferralRepository : IReferralRepository
{
    private const string Fn = "appointment.fn_appointment_referral";

    private readonly ProductDatabaseHelper _db;
    private readonly ILogger<ReferralRepository> _logger;

    public ReferralRepository(ProductDatabaseHelper db, ILogger<ReferralRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<ReferralDto?> GetCustomerDefaultAsync(
        int orgId, int appId, long accountId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("get_customer_default", orgId, appId, accountId: accountId);
            return DeserializeOrNull(json);
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} get_customer_default", Fn);
            throw;
        }
    }

    public async Task<ReferralDto> UpsertCustomerDefaultAsync(
        int orgId, int appId, long accountId, long userId, UpsertReferralRequest request,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "upsert_customer_default", orgId, appId,
                accountId: accountId,
                referredByType: request.ReferredByType,
                referredByEmployeeId: request.ReferredByEmployeeId,
                referredByAccountId: request.ReferredByAccountId,
                referredByName: request.ReferredByName,
                referredByNotes: request.ReferredByNotes,
                createdBy: userId);

            return DeserializeRequired(json, "Customer referral");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} upsert_customer_default", Fn);
            throw;
        }
    }

    public async Task ClearCustomerDefaultAsync(
        int orgId, int appId, long accountId, long userId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            await CallAsync("clear_customer_default", orgId, appId, accountId: accountId, createdBy: userId);
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} clear_customer_default", Fn);
            throw;
        }
    }

    public async Task<ReferralDto?> GetByAppointmentAsync(
        int orgId, int appId, long appointmentId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("get_by_appointment", orgId, appId, appointmentId: appointmentId);
            return DeserializeOrNull(json);
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} get_by_appointment", Fn);
            throw;
        }
    }

    public async Task<ReferralDto> UpsertForAppointmentAsync(
        int orgId, int appId, long appointmentId, long userId, UpsertReferralRequest request,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "upsert_for_appointment", orgId, appId,
                accountId: request.AccountId,
                appointmentId: appointmentId,
                referredByType: request.ReferredByType,
                referredByEmployeeId: request.ReferredByEmployeeId,
                referredByAccountId: request.ReferredByAccountId,
                referredByName: request.ReferredByName,
                referredByNotes: request.ReferredByNotes,
                createdBy: userId);

            return DeserializeRequired(json, "Appointment referral");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} upsert_for_appointment", Fn);
            throw;
        }
    }

    public async Task ClearForAppointmentAsync(
        int orgId, int appId, long appointmentId, long userId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            await CallAsync("clear_for_appointment", orgId, appId, appointmentId: appointmentId, createdBy: userId);
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} clear_for_appointment", Fn);
            throw;
        }
    }

    private Task<string> CallAsync(
        string action,
        int orgId,
        int appId,
        long? accountId = null,
        long? appointmentId = null,
        string? referredByType = null,
        long? referredByEmployeeId = null,
        long? referredByAccountId = null,
        string? referredByName = null,
        string? referredByNotes = null,
        long? createdBy = null) =>
        _db.ExecuteJsonFunctionAsync(
            Fn,
            Varchar(action),
            Int(orgId),
            Int(appId),
            Bigint(accountId),
            Bigint(appointmentId),
            Varchar(referredByType),
            Bigint(referredByEmployeeId),
            Bigint(referredByAccountId),
            Varchar(referredByName),
            Varchar(referredByNotes),
            Bigint(createdBy));

    private static ReferralDto? DeserializeOrNull(string json)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "null")
            return null;
        return JsonSerializer.Deserialize<ReferralDto>(json, PostgresJsonOptions.Options);
    }

    private static ReferralDto DeserializeRequired(string json, string label)
    {
        var dto = DeserializeOrNull(json);
        if (dto is null)
            throw new InvalidOperationException($"{label} response was empty.");
        return dto;
    }

    private static NpgsqlParameter Int(int value) =>
        new() { Value = value, NpgsqlDbType = NpgsqlDbType.Integer };

    private static NpgsqlParameter Bigint(long? value) =>
        new() { Value = value.HasValue ? value.Value : DBNull.Value, NpgsqlDbType = NpgsqlDbType.Bigint };

    private static NpgsqlParameter Varchar(string? value) =>
        new()
        {
            Value = string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim(),
            NpgsqlDbType = NpgsqlDbType.Varchar
        };
}
