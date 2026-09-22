using Appointment.Domain.DTOs.Services.Requests;
using Appointment.Domain.DTOs.Services.Responses;

namespace Appointment.Infrastructure.Repositories.Interfaces;

public interface IServiceRepository
{
    Task<IReadOnlyList<ServiceListItemDto>> GetServicesAsync(
        int orgId,
        int appId,
        GetServicesRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceDetailDto?> GetServiceByIdAsync(
        int orgId,
        int appId,
        int productId,
        CancellationToken cancellationToken = default);

    Task<CreateServiceResponse> CreateServiceAsync(
        int orgId,
        int appId,
        CreateServiceRequest request,
        CancellationToken cancellationToken = default);

    Task<CreateServiceResponse> UpdateServiceAsync(
        int orgId,
        int appId,
        int productId,
        UpdateServiceRequest request,
        CancellationToken cancellationToken = default);

    Task<CreateServiceResponse> DeactivateServiceAsync(
        int orgId,
        int appId,
        int productId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ServiceCategoryDto>> GetCategoriesAsync(
        int orgId,
        int appId,
        bool? isActive = true,
        CancellationToken cancellationToken = default);

    Task<ServiceCategoryDto> CreateCategoryAsync(
        int orgId,
        int appId,
        SaveServiceCategoryRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceCategoryDto> UpdateCategoryAsync(
        int orgId,
        int appId,
        int categoryId,
        UpdateServiceCategoryRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceCategoryDto> DeactivateCategoryAsync(
        int orgId,
        int appId,
        int categoryId,
        CancellationToken cancellationToken = default);
}
