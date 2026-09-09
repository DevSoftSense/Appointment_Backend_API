using Appointment.Domain.DTOs.Queue.Requests;
using Appointment.Domain.DTOs.Queue.Responses;

namespace Appointment.Infrastructure.Repositories.Interfaces;

public interface IQueueRepository
{
    Task<IReadOnlyList<QueueListItemDto>> GetQueueAsync(
        int orgId, int appId, GetQueueRequest request, CancellationToken cancellationToken = default);

    Task<QueueStatsDto> GetStatsAsync(
        int orgId, int appId, DateOnly? queueDate, long? professionalId, long? branchId,
        CancellationToken cancellationToken = default);

    Task<QueueListItemDto?> GetByIdAsync(
        int orgId, int appId, long queueEntryId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PendingCheckInDto>> GetPendingCheckInsAsync(
        int orgId, int appId, DateOnly? queueDate, long? professionalId, long? branchId,
        string? search, int limit, CancellationToken cancellationToken = default);

    Task<QueueListItemDto> CheckInAsync(
        int orgId, int appId, long createdBy, QueueCheckInRequest request,
        CancellationToken cancellationToken = default);

    Task<QueueListItemDto> AddWalkInAsync(
        int orgId, int appId, long createdBy, AddWalkInRequest request,
        CancellationToken cancellationToken = default);

    Task<QueueListItemDto> CallAsync(
        int orgId, int appId, long queueEntryId, CancellationToken cancellationToken = default);

    Task<QueueListItemDto> StartAsync(
        int orgId, int appId, long queueEntryId, CancellationToken cancellationToken = default);

    Task<QueueListItemDto> CompleteAsync(
        int orgId, int appId, long queueEntryId, long? updatedBy,
        CancellationToken cancellationToken = default);

    Task<QueueListItemDto> SkipAsync(
        int orgId, int appId, long queueEntryId, string? notes,
        CancellationToken cancellationToken = default);
}
