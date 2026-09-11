using Appointment.Application.Services.Interfaces;
using Appointment.Domain.DTOs.Cabins.Requests;
using Appointment.Domain.DTOs.Cabins.Responses;
using Appointment.Domain.Exceptions;
using Appointment.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Appointment.Application.Services.Classes;

public sealed class CabinService : ICabinService
{
    private readonly ICabinRepository _cabinRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CabinService> _logger;

    public CabinService(
        ICabinRepository cabinRepository,
        IConfiguration configuration,
        ILogger<CabinService> logger)
    {
        _cabinRepository = cabinRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<CabinListResponse> GetCabinsAsync(
        int orgId,
        GetCabinsRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        request ??= new GetCabinsRequest();
        if (request.Limit <= 0) request.Limit = 50;
        if (request.Offset < 0) request.Offset = 0;

        var appId = GetAppId();
        var items = await _cabinRepository.GetCabinsAsync(orgId, appId, request, cancellationToken);

        return new CabinListResponse
        {
            Items = items,
            TotalCount = items.Count > 0 ? items[0].TotalCount : 0
        };
    }

    public async Task<CabinDetailDto?> GetCabinByIdAsync(
        int orgId,
        long cabinResourceId,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (cabinResourceId <= 0)
            throw new ArgumentException("Cabin id is required.", nameof(cabinResourceId));

        var appId = GetAppId();
        return await _cabinRepository.GetCabinByIdAsync(orgId, appId, cabinResourceId, cancellationToken);
    }

    public async Task<CabinDetailDto> CreateCabinAsync(
        int orgId,
        CreateCabinRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.ResourceName))
            throw new ArgumentException("Cabin / room name is required.");

        var appId = GetAppId();

        try
        {
            return await _cabinRepository.CreateCabinAsync(orgId, appId, request, cancellationToken);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            _logger.LogInformation(ex, "Duplicate cabin for orgId {OrgId}", orgId);
            throw new CabinDuplicateException(
                "Cabin / room with this name or code already exists for this organisation");
        }
    }

    public async Task<CabinDetailDto> UpdateCabinAsync(
        int orgId,
        long cabinResourceId,
        UpdateCabinRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (cabinResourceId <= 0)
            throw new ArgumentException("Cabin id is required.", nameof(cabinResourceId));
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.ResourceName))
            throw new ArgumentException("Cabin / room name is required.");

        var appId = GetAppId();

        try
        {
            return await _cabinRepository.UpdateCabinAsync(orgId, appId, cabinResourceId, request, cancellationToken);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            _logger.LogInformation(ex, "Duplicate cabin on update for orgId {OrgId}", orgId);
            throw new CabinDuplicateException(
                "Cabin / room with this name already exists for this organisation");
        }
    }

    public async Task<CabinDetailDto> DeactivateCabinAsync(
        int orgId,
        long cabinResourceId,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (cabinResourceId <= 0)
            throw new ArgumentException("Cabin id is required.", nameof(cabinResourceId));

        var appId = GetAppId();
        return await _cabinRepository.DeactivateCabinAsync(orgId, appId, cabinResourceId, cancellationToken);
    }

    public async Task<CabinStatsDto> GetStatsAsync(
        int orgId,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        var appId = GetAppId();
        return await _cabinRepository.GetStatsAsync(orgId, appId, cancellationToken);
    }

    public async Task<IReadOnlyList<CabinBranchDto>> GetBranchesAsync(
        int orgId,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        var appId = GetAppId();
        return await _cabinRepository.GetBranchesAsync(orgId, appId, cancellationToken);
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
