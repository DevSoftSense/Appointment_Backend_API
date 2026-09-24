using System.Text.Json;
using Appointment.Domain.DTOs.Menu.Responses;
using Appointment.Infrastructure.Data;
using Appointment.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Appointment.Infrastructure.Repositories.Classes;

/// <summary>
/// Calls appointment.fn_appointment_menu on SOC_SaaS_Product. No inline table SQL.
/// </summary>
public sealed class MenuRepository : IMenuRepository
{
    private const string Fn = "appointment.fn_appointment_menu";

    private readonly ProductDatabaseHelper _db;
    private readonly ILogger<MenuRepository> _logger;

    public MenuRepository(ProductDatabaseHelper db, ILogger<MenuRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<MenuItemDto>> ListSidebarAsync(
        int appId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await _db.ExecuteJsonFunctionAsync(
                Fn,
                new NpgsqlParameter { Value = "list_sidebar", NpgsqlDbType = NpgsqlDbType.Varchar },
                new NpgsqlParameter { Value = appId, NpgsqlDbType = NpgsqlDbType.Integer });

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return [];

            return JsonSerializer.Deserialize<List<MenuItemDto>>(json, PostgresJsonOptions.Options) ?? [];
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} list_sidebar for appId {AppId}", Fn, appId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize list_sidebar for appId {AppId}", appId);
            throw new InvalidOperationException("Sidebar menu could not be parsed.", ex);
        }
    }
}
