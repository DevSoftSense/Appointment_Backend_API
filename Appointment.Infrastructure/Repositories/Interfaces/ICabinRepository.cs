using Appointment.Domain.DTOs.Cabins.Requests;
using Appointment.Domain.DTOs.Cabins.Responses;

namespace Appointment.Infrastructure.Repositories.Interfaces;

public interface ICabinRepository
{
    Task<IReadOnlyList<CabinListItemDto>> GetCabinsAsync(
        int orgId,
        int appId,
        GetCabinsRequest request,
        CancellationToken cancellationToken = default);

    Task<CabinDetailDto?> GetCabinByIdAsync(
        int orgId,
        int appId,
        long cabinResourceId,
        CancellationToken cancellationToken = default);

    Task<CabinDetailDto> CreateCabinAsync(
        int orgId,
        int appId,
        CreateCabinRequest request,
        CancellationToken cancellationToken = default);

    Task<CabinDetailDto> UpdateCabinAsync(
        int orgId,
        int appId,
        long cabinResourceId,
        UpdateCabinRequest request,
        CancellationToken cancellationToken = default);

    Task<CabinDetailDto> DeactivateCabinAsync(
        int orgId,
        int appId,
        long cabinResourceId,
        CancellationToken cancellationToken = default);

    Task<CabinStatsDto> GetStatsAsync(
        int orgId,
        int appId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CabinBranchDto>> GetBranchesAsync(
        int orgId,
        int appId,
        CancellationToken cancellationToken = default);
}
