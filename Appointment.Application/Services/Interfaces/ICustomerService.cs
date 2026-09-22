using Appointment.Domain.DTOs.Customers.Requests;
using Appointment.Domain.DTOs.Customers.Responses;
using Microsoft.AspNetCore.Http;

namespace Appointment.Application.Services.Interfaces;

public interface ICustomerService
{
    Task<CustomerListResponse> GetCustomersAsync(
        int orgId,
        GetCustomersRequest request,
        CancellationToken cancellationToken = default);

    Task<CustomerDetailDto?> GetCustomerByIdAsync(
        int orgId,
        int accountId,
        CancellationToken cancellationToken = default);

    Task<CustomerDetailDto?> FindCustomerByPhoneAsync(
        int orgId,
        string phoneMobile,
        CancellationToken cancellationToken = default);

    Task<CreateCustomerResponse> CreateCustomerAsync(
        int orgId,
        int createdBy,
        CreateCustomerRequest request,
        CancellationToken cancellationToken = default);

    Task<CreateCustomerResponse> UpdateCustomerAsync(
        int orgId,
        int accountId,
        UpdateCustomerRequest request,
        CancellationToken cancellationToken = default);

    Task<DeactivateCustomerResponse> DeactivateCustomerAsync(
        int orgId,
        int accountId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AccountTypeDto>> GetAccountTypesAsync(
        int orgId,
        CancellationToken cancellationToken = default);

    Task<CustomerStatsDto> GetCustomerStatsAsync(
        int orgId,
        CancellationToken cancellationToken = default);

    Task<CustomerDetailDto> UploadProfilePhotoAsync(
        int orgId,
        int accountId,
        IFormFile file,
        CancellationToken cancellationToken = default);

    Task<CustomerDetailDto> ClearProfilePhotoAsync(
        int orgId,
        int accountId,
        CancellationToken cancellationToken = default);
}
