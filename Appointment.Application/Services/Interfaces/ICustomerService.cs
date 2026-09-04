using Appointment.Domain.DTOs.Customers.Requests;
using Appointment.Domain.DTOs.Customers.Responses;

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
}
