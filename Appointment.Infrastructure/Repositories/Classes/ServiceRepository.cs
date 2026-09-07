using System.Text.Json;
using Appointment.Domain.DTOs.Services.Requests;
using Appointment.Domain.DTOs.Services.Responses;
using Appointment.Infrastructure.Data;
using Appointment.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Appointment.Infrastructure.Repositories.Classes;

/// <summary>
/// Calls appointment.fn_appointment_service(p_action, …) on SOC_SaaS_Product. No inline table SQL.
/// </summary>
public sealed class ServiceRepository : IServiceRepository
{
    private const string Fn = "appointment.fn_appointment_service";

    private readonly ProductDatabaseHelper _db;
    private readonly ILogger<ServiceRepository> _logger;

    public ServiceRepository(ProductDatabaseHelper db, ILogger<ServiceRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ServiceListItemDto>> GetServicesAsync(
        int orgId,
        int appId,
        GetServicesRequest request,
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
                isActive: request.IsActive,
                limit: request.Limit,
                offset: request.Offset);

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return [];

            return JsonSerializer.Deserialize<List<ServiceListItemDto>>(json, PostgresJsonOptions.Options)
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
            throw new InvalidOperationException("Service list response could not be parsed.", ex);
        }
    }

    public async Task<ServiceDetailDto?> GetServiceByIdAsync(
        int orgId,
        int appId,
        int productId,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var json = await CallAsync("get_by_id", orgId, appId, productId: productId);

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return null;

            return JsonSerializer.Deserialize<ServiceDetailDto>(json, PostgresJsonOptions.Options);
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} get_by_id for productId {ProductId}", Fn, productId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize {Fn} get_by_id for productId {ProductId}", Fn, productId);
            throw new InvalidOperationException("Service response could not be parsed.", ex);
        }
    }

    public async Task<CreateServiceResponse> CreateServiceAsync(
        int orgId,
        int appId,
        CreateServiceRequest request,
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
                productName: request.ProductName,
                categoryId: request.CategoryId,
                sellingPrice: request.SellingPrice,
                durationMinutes: request.DurationMinutes,
                salesDescription: request.SalesDescription,
                setIsActive: request.IsActive ?? true);

            return JsonSerializer.Deserialize<CreateServiceResponse>(json, PostgresJsonOptions.Options)
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
            throw new InvalidOperationException("Create service response could not be parsed.", ex);
        }
    }

    public async Task<CreateServiceResponse> UpdateServiceAsync(
        int orgId,
        int appId,
        int productId,
        UpdateServiceRequest request,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var json = await CallAsync(
                "update",
                orgId,
                appId,
                productId: productId,
                productName: request.ProductName,
                categoryId: request.CategoryId,
                sellingPrice: request.SellingPrice,
                durationMinutes: request.DurationMinutes,
                salesDescription: request.SalesDescription,
                setIsActive: request.IsActive);

            return JsonSerializer.Deserialize<CreateServiceResponse>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException($"{Fn} update returned no data");
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "PostgreSQL error in {Fn} update for productId {ProductId}", Fn, productId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize {Fn} update for productId {ProductId}", Fn, productId);
            throw new InvalidOperationException("Update service response could not be parsed.", ex);
        }
    }

    public async Task<CreateServiceResponse> DeactivateServiceAsync(
        int orgId,
        int appId,
        int productId,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var json = await CallAsync("deactivate", orgId, appId, productId: productId);

            return JsonSerializer.Deserialize<CreateServiceResponse>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException($"{Fn} deactivate returned no data");
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "PostgreSQL error in {Fn} deactivate for productId {ProductId}", Fn, productId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize {Fn} deactivate for productId {ProductId}", Fn, productId);
            throw new InvalidOperationException("Deactivate service response could not be parsed.", ex);
        }
    }

    public async Task<IReadOnlyList<ServiceCategoryDto>> GetCategoriesAsync(
        int orgId,
        int appId,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var json = await CallAsync("categories", orgId, appId);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return [];
            return JsonSerializer.Deserialize<List<ServiceCategoryDto>>(json, PostgresJsonOptions.Options) ?? [];
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} categories for orgId {OrgId}", Fn, orgId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize service categories for orgId {OrgId}", orgId);
            throw new InvalidOperationException("Category list could not be parsed.", ex);
        }
    }

    private Task<string> CallAsync(
        string action,
        int orgId,
        int appId,
        int? productId = null,
        string? search = null,
        bool? isActive = null,
        int? limit = null,
        int? offset = null,
        int? fiscalYearId = null,
        string? productName = null,
        int? categoryId = null,
        decimal? sellingPrice = null,
        int? durationMinutes = null,
        string? salesDescription = null,
        bool? setIsActive = null) =>
        _db.ExecuteJsonFunctionAsync(
            Fn,
            Varchar(action),
            Int(orgId),
            Int(appId),
            NullableInt(productId),
            Varchar(search),
            Bool(isActive),
            NullableInt(limit),
            NullableInt(offset),
            NullableInt(fiscalYearId),
            Varchar(productName),
            NullableInt(categoryId),
            Numeric(sellingPrice),
            NullableInt(durationMinutes),
            Text(salesDescription),
            Bool(setIsActive));

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

    private static NpgsqlParameter Numeric(decimal? value) =>
        new()
        {
            Value = value.HasValue ? value.Value : DBNull.Value,
            NpgsqlDbType = NpgsqlDbType.Numeric
        };

    private static NpgsqlParameter Bool(bool? value) =>
        new()
        {
            Value = value.HasValue ? value.Value : DBNull.Value,
            NpgsqlDbType = NpgsqlDbType.Boolean
        };
}
