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
/// Calls appointment.fn_appointment_* on SOC_SaaS_Product. No inline table SQL.
/// </summary>
public sealed class ServiceRepository : IServiceRepository
{
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
            var json = await _db.ExecuteTableFunctionAsJsonArrayAsync(
                "appointment.fn_appointment_get_services",
                Int(orgId),
                Int(appId),
                Varchar(request.Search),
                Bool(request.IsActive),
                Int(request.Limit),
                Int(request.Offset));

            return JsonSerializer.Deserialize<List<ServiceListItemDto>>(json, PostgresJsonOptions.Options)
                   ?? [];
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in fn_appointment_get_services for orgId {OrgId}", orgId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize fn_appointment_get_services for orgId {OrgId}", orgId);
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
            var json = await _db.ExecuteSingleRowTableFunctionAsJsonAsync(
                "appointment.fn_appointment_get_service_by_id",
                Int(orgId),
                Int(appId),
                Int(productId));

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return null;

            return JsonSerializer.Deserialize<ServiceDetailDto>(json, PostgresJsonOptions.Options);
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in fn_appointment_get_service_by_id for productId {ProductId}", productId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize fn_appointment_get_service_by_id for productId {ProductId}", productId);
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
            var json = await _db.ExecuteSingleRowTableFunctionAsJsonAsync(
                "appointment.fn_appointment_create_service",
                Int(orgId),
                Int(appId),
                NullableInt(request.FiscalYearId),
                Varchar(request.ProductName),
                NullableInt(request.CategoryId),
                Numeric(request.SellingPrice),
                NullableInt(request.DurationMinutes),
                Text(request.SalesDescription),
                Bool(request.IsActive ?? true));

            return JsonSerializer.Deserialize<CreateServiceResponse>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("fn_appointment_create_service returned no data");
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "PostgreSQL error in fn_appointment_create_service for orgId {OrgId}", orgId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize fn_appointment_create_service for orgId {OrgId}", orgId);
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
            var json = await _db.ExecuteSingleRowTableFunctionAsJsonAsync(
                "appointment.fn_appointment_update_service",
                Int(orgId),
                Int(appId),
                Int(productId),
                Varchar(request.ProductName),
                NullableInt(request.CategoryId),
                Numeric(request.SellingPrice),
                NullableInt(request.DurationMinutes),
                Text(request.SalesDescription),
                Bool(request.IsActive));

            return JsonSerializer.Deserialize<CreateServiceResponse>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("fn_appointment_update_service returned no data");
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "PostgreSQL error in fn_appointment_update_service for productId {ProductId}", productId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize fn_appointment_update_service for productId {ProductId}", productId);
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
            var json = await _db.ExecuteSingleRowTableFunctionAsJsonAsync(
                "appointment.fn_appointment_deactivate_service",
                Int(orgId),
                Int(appId),
                Int(productId));

            return JsonSerializer.Deserialize<CreateServiceResponse>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("fn_appointment_deactivate_service returned no data");
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "PostgreSQL error in fn_appointment_deactivate_service for productId {ProductId}", productId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize fn_appointment_deactivate_service for productId {ProductId}", productId);
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
            var json = await _db.ExecuteTableFunctionAsJsonArrayAsync(
                "appointment.fn_appointment_get_service_categories",
                Int(orgId),
                Int(appId));

            return JsonSerializer.Deserialize<List<ServiceCategoryDto>>(json, PostgresJsonOptions.Options) ?? [];
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in fn_appointment_get_service_categories for orgId {OrgId}", orgId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize service categories for orgId {OrgId}", orgId);
            throw new InvalidOperationException("Category list could not be parsed.", ex);
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
