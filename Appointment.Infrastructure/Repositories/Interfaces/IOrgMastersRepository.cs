using Appointment.Domain.DTOs.OrgMasters.Requests;
using Appointment.Domain.DTOs.OrgMasters.Responses;

namespace Appointment.Infrastructure.Repositories.Interfaces;

public interface IOrgMastersRepository
{
    Task<IReadOnlyList<BranchDetailDto>> ListBranchesAsync(
        int orgId, int appId, GetBranchesRequest request, CancellationToken cancellationToken = default);

    Task<BranchDetailDto> GetBranchAsync(
        int orgId, int appId, int branchId, CancellationToken cancellationToken = default);

    Task<BranchDetailDto> CreateBranchAsync(
        int orgId, int appId, SaveBranchRequest request, CancellationToken cancellationToken = default);

    Task<BranchDetailDto> UpdateBranchAsync(
        int orgId, int appId, int branchId, UpdateBranchRequest request, CancellationToken cancellationToken = default);

    Task DeactivateBranchAsync(
        int orgId, int appId, int branchId, CancellationToken cancellationToken = default);
}
