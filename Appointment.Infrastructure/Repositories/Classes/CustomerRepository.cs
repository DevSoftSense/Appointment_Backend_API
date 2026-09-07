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
/// Calls appointment.fn_appointment_customer(p_action, …) on SOC_SaaS_Product. No inline table SQL.
/// </summary>
public sealed class CustomerRepository : ICustomerRepository
{
    private const string Fn = "appointment.fn_appointment_customer";

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
            var json = await CallAsync(
                "list",
                orgId,
                appId,
                search: request.Search,
                status: request.Status,
                limit: request.Limit,
                offset: request.Offset,
                accountTypeId: request.AccountTypeId);

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return [];

            return JsonSerializer.Deserialize<List<CustomerListItemDto>>(json, PostgresJsonOptions.Options)
                   ?? [];
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} list for orgId {OrgId}", Fn, orgId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize {Fn} list for orgId {OrgId}", Fn, orgId);
            throw new InvalidOperationException("Customer list response could not be parsed.", ex);
        }
    }

    public async Task<CustomerStatsDto> GetCustomerStatsAsync(
        int orgId,
        int appId,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var json = await CallAsync("stats", orgId, appId);

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return new CustomerStatsDto();

            return JsonSerializer.Deserialize<CustomerStatsDto>(json, PostgresJsonOptions.Options)
                   ?? new CustomerStatsDto();
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} stats for orgId {OrgId}", Fn, orgId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize {Fn} stats for orgId {OrgId}", Fn, orgId);
            throw new InvalidOperationException("Customer stats response could not be parsed.", ex);
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
            var json = await CallAsync("get_by_id", orgId, appId, accountId: accountId);

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return null;

            return JsonSerializer.Deserialize<CustomerDetailDto>(json, PostgresJsonOptions.Options);
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} get_by_id for accountId {AccountId}", Fn, accountId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize {Fn} get_by_id for accountId {AccountId}", Fn, accountId);
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
            var json = await CallAsync(
                "create",
                orgId,
                appId,
                fiscalYearId: request.FiscalYearId,
                createdBy: createdBy,
                displayName: request.DisplayName,
                firstName: request.FirstName,
                lastName: request.LastName,
                salutation: request.Salutation,
                email: request.Email,
                phoneMobile: request.PhoneMobile,
                phoneMobileAlt: request.PhoneMobileAlt,
                gender: request.Gender,
                dateOfBirth: request.DateOfBirth,
                accountTypeId: request.AccountTypeId,
                branchId: request.BranchId,
                partyType: request.PartyType,
                remarks: request.Remarks,
                status: string.IsNullOrWhiteSpace(request.Status) ? "active" : request.Status,
                phoneWork: request.PhoneWork,
                acquisitionSource: request.AcquisitionSource,
                anniversaryDate: request.AnniversaryDate,
                customerSince: request.CustomerSince,
                preferredLanguage: request.PreferredLanguage,
                companyName: request.CompanyName);

            return JsonSerializer.Deserialize<CreateCustomerResponse>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException($"{Fn} create returned no data");
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "PostgreSQL error in {Fn} create for orgId {OrgId}", Fn, orgId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize {Fn} create for orgId {OrgId}", Fn, orgId);
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
            var json = await CallAsync(
                "update",
                orgId,
                appId,
                accountId: accountId,
                displayName: request.DisplayName,
                firstName: request.FirstName,
                lastName: request.LastName,
                salutation: request.Salutation,
                email: request.Email,
                phoneMobile: request.PhoneMobile,
                phoneMobileAlt: request.PhoneMobileAlt,
                gender: request.Gender,
                dateOfBirth: request.DateOfBirth,
                accountTypeId: request.AccountTypeId,
                branchId: request.BranchId,
                partyType: request.PartyType,
                remarks: request.Remarks,
                status: request.Status,
                phoneWork: request.PhoneWork,
                acquisitionSource: request.AcquisitionSource,
                anniversaryDate: request.AnniversaryDate,
                customerSince: request.CustomerSince,
                preferredLanguage: request.PreferredLanguage,
                companyName: request.CompanyName);

            return JsonSerializer.Deserialize<CreateCustomerResponse>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException($"{Fn} update returned no data");
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "PostgreSQL error in {Fn} update for accountId {AccountId}", Fn, accountId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize {Fn} update for accountId {AccountId}", Fn, accountId);
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
            var json = await CallAsync("deactivate", orgId, appId, accountId: accountId);

            return JsonSerializer.Deserialize<DeactivateCustomerResponse>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException($"{Fn} deactivate returned no data");
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "PostgreSQL error in {Fn} deactivate for accountId {AccountId}", Fn, accountId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize {Fn} deactivate for accountId {AccountId}", Fn, accountId);
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
            var json = await CallAsync("account_types", orgId, appId);

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return [];

            return JsonSerializer.Deserialize<List<AccountTypeDto>>(json, PostgresJsonOptions.Options)
                   ?? [];
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} account_types for orgId {OrgId}", Fn, orgId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize account types for orgId {OrgId}", orgId);
            throw new InvalidOperationException("Account type list could not be parsed.", ex);
        }
    }

    private Task<string> CallAsync(
        string action,
        int orgId,
        int appId,
        int? accountId = null,
        string? search = null,
        string? status = null,
        int? limit = null,
        int? offset = null,
        int? fiscalYearId = null,
        int? createdBy = null,
        string? displayName = null,
        string? firstName = null,
        string? lastName = null,
        string? salutation = null,
        string? email = null,
        string? phoneMobile = null,
        string? phoneMobileAlt = null,
        string? gender = null,
        DateOnly? dateOfBirth = null,
        int? accountTypeId = null,
        int? branchId = null,
        string? partyType = null,
        string? remarks = null,
        string? phoneWork = null,
        string? acquisitionSource = null,
        DateOnly? anniversaryDate = null,
        DateOnly? customerSince = null,
        string? preferredLanguage = null,
        string? companyName = null) =>
        _db.ExecuteJsonFunctionAsync(
            Fn,
            Varchar(action),
            Int(orgId),
            Int(appId),
            NullableInt(accountId),
            Varchar(search),
            Varchar(status),
            NullableInt(limit),
            NullableInt(offset),
            NullableInt(fiscalYearId),
            NullableInt(createdBy),
            Varchar(displayName),
            Varchar(firstName),
            Varchar(lastName),
            Varchar(salutation),
            Varchar(email),
            Varchar(phoneMobile),
            Varchar(phoneMobileAlt),
            Varchar(gender),
            Date(dateOfBirth),
            NullableInt(accountTypeId),
            NullableInt(branchId),
            Varchar(partyType),
            Text(remarks),
            Varchar(phoneWork),
            Varchar(acquisitionSource),
            Date(anniversaryDate),
            Date(customerSince),
            Varchar(preferredLanguage),
            Varchar(companyName));

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
