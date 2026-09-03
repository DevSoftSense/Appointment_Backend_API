using Appointment.Domain.DTOs.Customers.Requests;
using Appointment.Domain.DTOs.Customers.Responses;

namespace Appointment.Infrastructure.Repositories.Interfaces;

public interface ICustomerRepository
{
    Task<IReadOnlyList<CustomerListItemDto>> GetCustomersAsync(
        int orgId,
        int productId,
        GetCustomersRequest request,
        CancellationToken cancellationToken = default);

    Task<CustomerDetailDto?> GetCustomerByIdAsync(
        int orgId,
        int productId,
        int accountId,
        CancellationToken cancellationToken = default);

    Task<CreateCustomerResponse> CreateCustomerAsync(
        int orgId,
        int productId,
        int? createdBy,
        CreateCustomerRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AccountTypeDto>> GetAccountTypesForCustomerAsync(
        int orgId,
        int productId,
        CancellationToken cancellationToken = default);
}
