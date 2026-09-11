using Appointment.Domain.DTOs.Cabins.Requests;
using Appointment.Domain.DTOs.Cabins.Responses;

namespace Appointment.Application.Services.Interfaces;

public interface ICabinService
{
    Task<CabinListResponse> GetCabinsAsync(
        int orgId,
        GetCabinsRequest request,
        CancellationToken cancellationToken = default);

    Task<CabinDetailDto?> GetCabinByIdAsync(
        int orgId,
        long cabinResourceId,
        CancellationToken cancellationToken = default);

    Task<CabinDetailDto> CreateCabinAsync(
        int orgId,
        CreateCabinRequest request,
        CancellationToken cancellationToken = default);

    Task<CabinDetailDto> UpdateCabinAsync(
        int orgId,
        long cabinResourceId,
        UpdateCabinRequest request,
        CancellationToken cancellationToken = default);

    Task<CabinDetailDto> DeactivateCabinAsync(
        int orgId,
        long cabinResourceId,
        CancellationToken cancellationToken = default);

    Task<CabinStatsDto> GetStatsAsync(
        int orgId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CabinBranchDto>> GetBranchesAsync(
        int orgId,
        CancellationToken cancellationToken = default);
}
