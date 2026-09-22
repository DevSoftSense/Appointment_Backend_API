using Appointment.Application.Services.Interfaces;
using Appointment.Domain.DTOs.Customers.Requests;
using Appointment.Domain.DTOs.Customers.Responses;
using Appointment.Domain.Exceptions;
using Appointment.Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Appointment.Application.Services.Classes;

public sealed class CustomerService : ICustomerService
{
    /// <summary>Max profile photo size (1 MB).</summary>
    private const long ProfilePhotoMaxBytes = 1024L * 1024L;

    private static readonly HashSet<string> PhotoExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".gif", ".webp"
    };

    private readonly ICustomerRepository _customerRepository;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(
        ICustomerRepository customerRepository,
        IWebHostEnvironment env,
        IConfiguration configuration,
        ILogger<CustomerService> logger)
    {
        _customerRepository = customerRepository;
        _env = env;
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
        foreach (var item in items)
            AttachPhotoUrl(item);

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
        var customer = await _customerRepository.GetCustomerByIdAsync(orgId, appId, accountId, cancellationToken);
        AttachPhotoUrl(customer);
        return customer;
    }

    public async Task<CustomerDetailDto?> FindCustomerByPhoneAsync(
        int orgId,
        string phoneMobile,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (string.IsNullOrWhiteSpace(phoneMobile))
            throw new ArgumentException("Phone number is required.", nameof(phoneMobile));

        var appId = GetAppId();
        var customer = await _customerRepository.FindCustomerByPhoneAsync(
            orgId, appId, phoneMobile.Trim(), cancellationToken);
        AttachPhotoUrl(customer);
        return customer;
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

    public async Task<CustomerStatsDto> GetCustomerStatsAsync(
        int orgId,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        var appId = GetAppId();
        return await _customerRepository.GetCustomerStatsAsync(orgId, appId, cancellationToken);
    }

    public async Task<CustomerDetailDto> UploadProfilePhotoAsync(
        int orgId,
        int accountId,
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (accountId <= 0)
            throw new ArgumentException("Customer id is required.", nameof(accountId));
        if (file is null || file.Length <= 0)
            throw new ArgumentException("A non-empty image file is required.");
        if (file.Length > ProfilePhotoMaxBytes)
            throw new ArgumentException("Profile photo must be 1 MB or smaller.");

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(ext) || !PhotoExtensions.Contains(ext))
            throw new ArgumentException("Only PNG, JPG, JPEG, GIF, or WEBP images are allowed.");

        var appId = GetAppId();
        var existing = await _customerRepository.GetCustomerByIdAsync(orgId, appId, accountId, cancellationToken);
        if (existing is null)
            throw new ArgumentException("Customer not found.");

        var relativeFolder = Path.Combine(
            "Uploads", "Customer_Photos", orgId.ToString(), accountId.ToString());
        var webRoot = string.IsNullOrWhiteSpace(_env.WebRootPath)
            ? Path.Combine(_env.ContentRootPath, "wwwroot")
            : _env.WebRootPath;
        var physicalFolder = Path.Combine(webRoot, relativeFolder);
        Directory.CreateDirectory(physicalFolder);

        var storedName = $"profile_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{ext.ToLowerInvariant()}";
        var physicalPath = Path.Combine(physicalFolder, storedName);
        var relativePath = Path.Combine(relativeFolder, storedName).Replace('\\', '/');

        await using (var stream = new FileStream(physicalPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        try
        {
            var saved = await _customerRepository.SetProfilePhotoAsync(
                orgId, appId, accountId, relativePath, cancellationToken)
                ?? throw new InvalidOperationException("Failed to save profile photo path.");

            TryDeletePhysical(existing.PartyProfile, webRoot);

            var updated = await _customerRepository.GetCustomerByIdAsync(
                orgId, appId, accountId, cancellationToken) ?? saved;
            AttachPhotoUrl(updated);
            return updated;
        }
        catch
        {
            TryDeletePhysical(relativePath, webRoot);
            throw;
        }
    }

    public async Task<CustomerDetailDto> ClearProfilePhotoAsync(
        int orgId,
        int accountId,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (accountId <= 0)
            throw new ArgumentException("Customer id is required.", nameof(accountId));

        var appId = GetAppId();
        var existing = await _customerRepository.GetCustomerByIdAsync(orgId, appId, accountId, cancellationToken);
        if (existing is null)
            throw new ArgumentException("Customer not found.");

        await _customerRepository.SetProfilePhotoAsync(
            orgId, appId, accountId, null, cancellationToken);

        var webRoot = string.IsNullOrWhiteSpace(_env.WebRootPath)
            ? Path.Combine(_env.ContentRootPath, "wwwroot")
            : _env.WebRootPath;
        TryDeletePhysical(existing.PartyProfile, webRoot);

        var updated = await _customerRepository.GetCustomerByIdAsync(orgId, appId, accountId, cancellationToken)
            ?? existing;
        updated.PartyProfile = null;
        AttachPhotoUrl(updated);
        return updated;
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

    private static void AttachPhotoUrl(CustomerDetailDto? customer)
    {
        if (customer is null) return;
        customer.ProfilePhotoUrl = ToPublicUrl(customer.PartyProfile);
    }

    private static void AttachPhotoUrl(CustomerListItemDto item)
    {
        item.ProfilePhotoUrl = ToPublicUrl(item.PartyProfile);
    }

    private static string ToPublicUrl(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return string.Empty;
        var path = relativePath.Replace('\\', '/').TrimStart('/');
        return "/" + path;
    }

    private void TryDeletePhysical(string? relativePath, string webRoot)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return;
        try
        {
            var physical = Path.Combine(webRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(physical))
                File.Delete(physical);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not delete old customer photo {Path}", relativePath);
        }
    }
}
