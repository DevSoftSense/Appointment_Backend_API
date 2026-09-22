using Appointment.Domain.DTOs.Services.Requests;
using Appointment.Domain.DTOs.Services.Responses;

namespace Appointment.Application.Services.Interfaces;

public interface IServiceCatalogService
{
    Task<ServiceListResponse> GetServicesAsync(
        int orgId,
        GetServicesRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceDetailDto?> GetServiceByIdAsync(
        int orgId,
        int productId,
        CancellationToken cancellationToken = default);

    Task<CreateServiceResponse> CreateServiceAsync(
        int orgId,
        CreateServiceRequest request,
        CancellationToken cancellationToken = default);

    Task<CreateServiceResponse> UpdateServiceAsync(
        int orgId,
        int productId,
        UpdateServiceRequest request,
        CancellationToken cancellationToken = default);

    Task<CreateServiceResponse> DeactivateServiceAsync(
        int orgId,
        int productId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ServiceCategoryDto>> GetCategoriesAsync(
        int orgId,
        bool? isActive = true,
        CancellationToken cancellationToken = default);

    Task<ServiceCategoryDto> CreateCategoryAsync(
        int orgId,
        SaveServiceCategoryRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceCategoryDto> UpdateCategoryAsync(
        int orgId,
        int categoryId,
        UpdateServiceCategoryRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceCategoryDto> DeactivateCategoryAsync(
        int orgId,
        int categoryId,
        CancellationToken cancellationToken = default);
}
