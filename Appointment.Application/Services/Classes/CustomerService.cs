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

        var productId = GetProductId();
        var items = await _customerRepository.GetCustomersAsync(orgId, productId, request, cancellationToken);

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

        var productId = GetProductId();
        return await _customerRepository.GetCustomerByIdAsync(orgId, productId, accountId, cancellationToken);
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

        var productId = GetProductId();

        try
        {
            return await _customerRepository.CreateCustomerAsync(
                orgId,
                productId,
                createdBy > 0 ? createdBy : null,
                request,
                cancellationToken);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            _logger.LogInformation(ex, "Duplicate customer email for orgId {OrgId}", orgId);
            throw new CustomerDuplicateEmailException(
                "Customer with this email already exists for this organisation and product");
        }
    }

    public async Task<IReadOnlyList<AccountTypeDto>> GetAccountTypesAsync(
        int orgId,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        var productId = GetProductId();
        return await _customerRepository.GetAccountTypesForCustomerAsync(orgId, productId, cancellationToken);
    }

    private int GetProductId()
    {
        var productId = _configuration.GetValue<int?>("Appointment:ProductId") ?? 0;
        if (productId <= 0)
            throw new InvalidOperationException("Appointment:ProductId is not configured.");
        return productId;
    }

    private static void ValidateOrg(int orgId)
    {
        if (orgId <= 0)
            throw new ArgumentException("Organisation ID is required.");
    }
}
