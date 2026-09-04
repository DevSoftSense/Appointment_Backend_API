using System.Text.Json;
using Appointment.Domain.DTOs.Customers.Requests;
using Appointment.Domain.DTOs.Customers.Responses;
using Appointment.Infrastructure.Data;
using Appointment.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Appointment.Infrastructure.Repositories.Classes;

/// <summary>
/// Calls appointment.fn_appointment_* on SOC_SaaS_Product. No inline table SQL.
/// </summary>
public sealed class CustomerRepository : ICustomerRepository
{
    private readonly ProductDatabaseHelper _db;
    private readonly ILogger<CustomerRepository> _logger;

    public CustomerRepository(ProductDatabaseHelper db, ILogger<CustomerRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<CustomerListItemDto>> GetCustomersAsync(
        int orgId,
        int appId,
        GetCustomersRequest request,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var json = await _db.ExecuteTableFunctionAsJsonArrayAsync(
                "appointment.fn_appointment_get_customers",
                Int(orgId),
                Int(appId),
                Varchar(request.Search),
                Varchar(request.Status),
                Int(request.Limit),
                Int(request.Offset));

            return JsonSerializer.Deserialize<List<CustomerListItemDto>>(json, PostgresJsonOptions.Options)
                   ?? [];
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in fn_appointment_get_customers for orgId {OrgId}", orgId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize fn_appointment_get_customers for orgId {OrgId}", orgId);
            throw new InvalidOperationException("Customer list response could not be parsed.", ex);
        }
    }

    public async Task<CustomerDetailDto?> GetCustomerByIdAsync(
        int orgId,
        int appId,
        int accountId,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var json = await _db.ExecuteSingleRowTableFunctionAsJsonAsync(
                "appointment.fn_appointment_get_customer_by_id",
                Int(orgId),
                Int(appId),
                Int(accountId));

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return null;

            return JsonSerializer.Deserialize<CustomerDetailDto>(json, PostgresJsonOptions.Options);
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in fn_appointment_get_customer_by_id for accountId {AccountId}", accountId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize fn_appointment_get_customer_by_id for accountId {AccountId}", accountId);
            throw new InvalidOperationException("Customer response could not be parsed.", ex);
        }
    }

    public async Task<CreateCustomerResponse> CreateCustomerAsync(
        int orgId,
        int appId,
        int? createdBy,
        CreateCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var json = await _db.ExecuteSingleRowTableFunctionAsJsonAsync(
                "appointment.fn_appointment_create_customer",
                Int(orgId),
                Int(appId),
                NullableInt(request.FiscalYearId),
                NullableInt(createdBy),
                Varchar(request.DisplayName),
                Varchar(request.FirstName),
                Varchar(request.LastName),
                Varchar(request.Salutation),
                Varchar(request.Email),
                Varchar(request.PhoneMobile),
                Varchar(request.PhoneMobileAlt),
                Varchar(request.Gender),
                Date(request.DateOfBirth),
                NullableInt(request.AccountTypeId),
                NullableInt(request.BranchId),
                Varchar(request.PartyType),
                Text(request.Remarks),
                Varchar(string.IsNullOrWhiteSpace(request.Status) ? "active" : request.Status));

            return JsonSerializer.Deserialize<CreateCustomerResponse>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("fn_appointment_create_customer returned no data");
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "PostgreSQL error in fn_appointment_create_customer for orgId {OrgId}", orgId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize fn_appointment_create_customer for orgId {OrgId}", orgId);
            throw new InvalidOperationException("Create customer response could not be parsed.", ex);
        }
    }

    public async Task<CreateCustomerResponse> UpdateCustomerAsync(
        int orgId,
        int appId,
        int accountId,
        UpdateCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var json = await _db.ExecuteSingleRowTableFunctionAsJsonAsync(
                "appointment.fn_appointment_update_customer",
                Int(orgId),
                Int(appId),
                Int(accountId),
                Varchar(request.DisplayName),
                Varchar(request.FirstName),
                Varchar(request.LastName),
                Varchar(request.Salutation),
                Varchar(request.Email),
                Varchar(request.PhoneMobile),
                Varchar(request.PhoneMobileAlt),
                Varchar(request.Gender),
                Date(request.DateOfBirth),
                NullableInt(request.AccountTypeId),
                NullableInt(request.BranchId),
                Varchar(request.PartyType),
                Text(request.Remarks),
                Varchar(request.Status));

            return JsonSerializer.Deserialize<CreateCustomerResponse>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("fn_appointment_update_customer returned no data");
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "PostgreSQL error in fn_appointment_update_customer for accountId {AccountId}", accountId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize fn_appointment_update_customer for accountId {AccountId}", accountId);
            throw new InvalidOperationException("Update customer response could not be parsed.", ex);
        }
    }

    public async Task<DeactivateCustomerResponse> DeactivateCustomerAsync(
        int orgId,
        int appId,
        int accountId,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var json = await _db.ExecuteSingleRowTableFunctionAsJsonAsync(
                "appointment.fn_appointment_deactivate_customer",
                Int(orgId),
                Int(appId),
                Int(accountId));

            return JsonSerializer.Deserialize<DeactivateCustomerResponse>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("fn_appointment_deactivate_customer returned no data");
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "PostgreSQL error in fn_appointment_deactivate_customer for accountId {AccountId}", accountId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize fn_appointment_deactivate_customer for accountId {AccountId}", accountId);
            throw new InvalidOperationException("Deactivate customer response could not be parsed.", ex);
        }
    }

    public async Task<IReadOnlyList<AccountTypeDto>> GetAccountTypesForCustomerAsync(
        int orgId,
        int appId,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var json = await _db.ExecuteTableFunctionAsJsonArrayAsync(
                "appointment.fn_appointment_get_account_types_for_customer",
                Int(orgId),
                Int(appId));

            return JsonSerializer.Deserialize<List<AccountTypeDto>>(json, PostgresJsonOptions.Options)
                   ?? [];
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in fn_appointment_get_account_types_for_customer for orgId {OrgId}", orgId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize account types for orgId {OrgId}", orgId);
            throw new InvalidOperationException("Account type list could not be parsed.", ex);
        }
    }

    private static NpgsqlParameter Int(int value) =>
        new() { Value = value, NpgsqlDbType = NpgsqlDbType.Integer };

    private static NpgsqlParameter NullableInt(int? value) =>
        new() { Value = value.HasValue ? value.Value : DBNull.Value, NpgsqlDbType = NpgsqlDbType.Integer };

    private static NpgsqlParameter Varchar(string? value) =>
        new()
        {
            Value = string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim(),
            NpgsqlDbType = NpgsqlDbType.Varchar
        };

    private static NpgsqlParameter Text(string? value) =>
        new()
        {
            Value = string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim(),
            NpgsqlDbType = NpgsqlDbType.Text
        };

    private static NpgsqlParameter Date(DateOnly? value) =>
        new()
        {
            Value = value.HasValue ? value.Value : DBNull.Value,
            NpgsqlDbType = NpgsqlDbType.Date
        };
}
