using Appointment.Domain.DTOs.Customers.Requests;
using Appointment.Domain.DTOs.Customers.Responses;

namespace Appointment.Infrastructure.Repositories.Interfaces;

public interface ICustomerRepository
{
    Task<IReadOnlyList<CustomerListItemDto>> GetCustomersAsync(
        int orgId,
        int appId,
        GetCustomersRequest request,
        CancellationToken cancellationToken = default);

    Task<CustomerDetailDto?> GetCustomerByIdAsync(
        int orgId,
        int appId,
        int accountId,
        CancellationToken cancellationToken = default);

    Task<CreateCustomerResponse> CreateCustomerAsync(
        int orgId,
        int appId,
        int? createdBy,
        CreateCustomerRequest request,
        CancellationToken cancellationToken = default);

    Task<CreateCustomerResponse> UpdateCustomerAsync(
        int orgId,
        int appId,
        int accountId,
        UpdateCustomerRequest request,
        CancellationToken cancellationToken = default);

    Task<DeactivateCustomerResponse> DeactivateCustomerAsync(
        int orgId,
        int appId,
        int accountId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AccountTypeDto>> GetAccountTypesForCustomerAsync(
        int orgId,
        int appId,
        CancellationToken cancellationToken = default);
}
