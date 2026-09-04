using Appointment.Application.Services.Interfaces;
using Appointment.Domain.DTOs.Services.Requests;
using Appointment.Domain.DTOs.Services.Responses;
using Appointment.Domain.Exceptions;
using Appointment.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Appointment.Application.Services.Classes;

/// <summary>
/// Appointment treatment/service catalog (public.tab_product_master).
/// Named ServiceCatalogService to avoid clash with Microsoft DI IService*.
/// </summary>
public sealed class ServiceCatalogService : IServiceCatalogService
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ServiceCatalogService> _logger;

    public ServiceCatalogService(
        IServiceRepository serviceRepository,
        IConfiguration configuration,
        ILogger<ServiceCatalogService> logger)
    {
        _serviceRepository = serviceRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ServiceListResponse> GetServicesAsync(
        int orgId,
        GetServicesRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        request ??= new GetServicesRequest();
        if (request.Limit <= 0) request.Limit = 50;
        if (request.Offset < 0) request.Offset = 0;

        var appId = GetAppId();
        var items = await _serviceRepository.GetServicesAsync(orgId, appId, request, cancellationToken);

        return new ServiceListResponse
        {
            Items = items,
            TotalCount = items.Count > 0 ? items[0].TotalCount : 0
        };
    }

    public async Task<ServiceDetailDto?> GetServiceByIdAsync(
        int orgId,
        int productId,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (productId <= 0)
            throw new ArgumentException("Service id is required.", nameof(productId));

        var appId = GetAppId();
        return await _serviceRepository.GetServiceByIdAsync(orgId, appId, productId, cancellationToken);
    }

    public async Task<CreateServiceResponse> CreateServiceAsync(
        int orgId,
        CreateServiceRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.ProductName))
            throw new ArgumentException("Service name is required.");

        if (request.DurationMinutes.HasValue && request.DurationMinutes.Value <= 0)
            throw new ArgumentException("Duration must be greater than 0 minutes.");

        if (request.SellingPrice.HasValue && request.SellingPrice.Value < 0)
            throw new ArgumentException("Price cannot be negative.");

        var appId = GetAppId();

        try
        {
            return await _serviceRepository.CreateServiceAsync(orgId, appId, request, cancellationToken);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            _logger.LogInformation(ex, "Duplicate service name for orgId {OrgId}", orgId);
            throw new ServiceDuplicateNameException(
                "Service with this name already exists for this organisation and app");
        }
    }

    public async Task<CreateServiceResponse> UpdateServiceAsync(
        int orgId,
        int productId,
        UpdateServiceRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (productId <= 0)
            throw new ArgumentException("Service id is required.", nameof(productId));
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.ProductName))
            throw new ArgumentException("Service name is required.");

        if (request.DurationMinutes.HasValue && request.DurationMinutes.Value <= 0)
            throw new ArgumentException("Duration must be greater than 0 minutes.");

        if (request.SellingPrice.HasValue && request.SellingPrice.Value < 0)
            throw new ArgumentException("Price cannot be negative.");

        var appId = GetAppId();

        try
        {
            return await _serviceRepository.UpdateServiceAsync(orgId, appId, productId, request, cancellationToken);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            _logger.LogInformation(ex, "Duplicate service name on update for orgId {OrgId}", orgId);
            throw new ServiceDuplicateNameException(
                "Service with this name already exists for this organisation and app");
        }
    }

    public async Task<CreateServiceResponse> DeactivateServiceAsync(
        int orgId,
        int productId,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (productId <= 0)
            throw new ArgumentException("Service id is required.", nameof(productId));

        var appId = GetAppId();
        return await _serviceRepository.DeactivateServiceAsync(orgId, appId, productId, cancellationToken);
    }

    public async Task<IReadOnlyList<ServiceCategoryDto>> GetCategoriesAsync(
        int orgId,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        var appId = GetAppId();
        return await _serviceRepository.GetCategoriesAsync(orgId, appId, cancellationToken);
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
