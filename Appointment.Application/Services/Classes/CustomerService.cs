using Appointment.Application.Services.Interfaces;
using Appointment.Domain.DTOs.Customers.Requests;
using Appointment.Domain.DTOs.Customers.Responses;
using Appointment.Domain.Exceptions;
using Appointment.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Appointment.Application.Services.Classes;

public sealed class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(
        ICustomerRepository customerRepository,
        IConfiguration configuration,
        ILogger<CustomerService> logger)
    {
        _customerRepository = customerRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<CustomerListResponse> GetCustomersAsync(
        int orgId,
        GetCustomersRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        request ??= new GetCustomersRequest();
        if (request.Limit <= 0) request.Limit = 50;
        if (request.Offset < 0) request.Offset = 0;

        var appId = GetAppId();
        var items = await _customerRepository.GetCustomersAsync(orgId, appId, request, cancellationToken);

        return new CustomerListResponse
        {
            Items = items,
            TotalCount = items.Count > 0 ? items[0].TotalCount : 0
        };
    }

    public async Task<CustomerDetailDto?> GetCustomerByIdAsync(
        int orgId,
        int accountId,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (accountId <= 0)
            throw new ArgumentException("Customer id is required.", nameof(accountId));

        var appId = GetAppId();
        return await _customerRepository.GetCustomerByIdAsync(orgId, appId, accountId, cancellationToken);
    }

    public async Task<CreateCustomerResponse> CreateCustomerAsync(
        int orgId,
        int createdBy,
        CreateCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        var hasDisplayName = !string.IsNullOrWhiteSpace(request.DisplayName);
        var hasNameParts = !string.IsNullOrWhiteSpace(request.FirstName)
                           || !string.IsNullOrWhiteSpace(request.LastName);

        if (!hasDisplayName && !hasNameParts)
            throw new ArgumentException("Name is required. Provide displayName or firstName.");

        if (!string.IsNullOrWhiteSpace(request.Email) && !request.Email.Contains('@'))
            throw new ArgumentException("Email is not valid.");

        var appId = GetAppId();

        try
        {
            return await _customerRepository.CreateCustomerAsync(
                orgId,
                appId,
                createdBy > 0 ? createdBy : null,
                request,
                cancellationToken);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            _logger.LogInformation(ex, "Duplicate customer email for orgId {OrgId}", orgId);
            throw new CustomerDuplicateEmailException(
                "Customer with this email already exists for this organisation and app");
        }
    }

    public async Task<CreateCustomerResponse> UpdateCustomerAsync(
        int orgId,
        int accountId,
        UpdateCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (accountId <= 0)
            throw new ArgumentException("Customer id is required.", nameof(accountId));
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        var hasDisplayName = !string.IsNullOrWhiteSpace(request.DisplayName);
        var hasNameParts = !string.IsNullOrWhiteSpace(request.FirstName)
                           || !string.IsNullOrWhiteSpace(request.LastName);

        if (!hasDisplayName && !hasNameParts)
            throw new ArgumentException("Name is required. Provide displayName or firstName.");

        if (!string.IsNullOrWhiteSpace(request.Email) && !request.Email.Contains('@'))
            throw new ArgumentException("Email is not valid.");

        var appId = GetAppId();

        try
        {
            return await _customerRepository.UpdateCustomerAsync(orgId, appId, accountId, request, cancellationToken);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            _logger.LogInformation(ex, "Duplicate customer email on update for orgId {OrgId}", orgId);
            throw new CustomerDuplicateEmailException(
                "Customer with this email already exists for this organisation and app");
        }
    }

    public async Task<DeactivateCustomerResponse> DeactivateCustomerAsync(
        int orgId,
        int accountId,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (accountId <= 0)
            throw new ArgumentException("Customer id is required.", nameof(accountId));

        var appId = GetAppId();
        return await _customerRepository.DeactivateCustomerAsync(orgId, appId, accountId, cancellationToken);
    }

    public async Task<IReadOnlyList<AccountTypeDto>> GetAccountTypesAsync(
        int orgId,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        var appId = GetAppId();
        return await _customerRepository.GetAccountTypesForCustomerAsync(orgId, appId, cancellationToken);
    }

    /// <summary>
    /// SOC app id for Appointment (public.app_id). Prefers Appointment:AppId; falls back to ProductId.
    /// </summary>
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
