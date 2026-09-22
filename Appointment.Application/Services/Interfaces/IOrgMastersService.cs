using Appointment.Domain.DTOs.OrgMasters.Requests;
using Appointment.Domain.DTOs.OrgMasters.Responses;

namespace Appointment.Application.Services.Interfaces;

public interface IOrgMastersService
{
    Task<IReadOnlyList<BranchDetailDto>> ListBranchesAsync(
        int orgId, GetBranchesRequest request, CancellationToken cancellationToken = default);

    Task<BranchDetailDto> GetBranchAsync(
        int orgId, int branchId, CancellationToken cancellationToken = default);

    Task<BranchDetailDto> CreateBranchAsync(
        int orgId, SaveBranchRequest request, CancellationToken cancellationToken = default);

    Task<BranchDetailDto> UpdateBranchAsync(
        int orgId, int branchId, UpdateBranchRequest request, CancellationToken cancellationToken = default);

    Task DeactivateBranchAsync(
        int orgId, int branchId, CancellationToken cancellationToken = default);
}
