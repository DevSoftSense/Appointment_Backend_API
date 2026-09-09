using Appointment.Domain.DTOs.Queue.Requests;
using Appointment.Domain.DTOs.Queue.Responses;

namespace Appointment.Application.Services.Interfaces;

public interface IQueueService
{
    Task<QueueListResponse> GetQueueAsync(int orgId, GetQueueRequest request, CancellationToken cancellationToken = default);

    Task<QueueStatsDto> GetStatsAsync(
        int orgId, DateOnly? queueDate, long? professionalId, long? branchId,
        CancellationToken cancellationToken = default);

    Task<QueueListItemDto?> GetByIdAsync(int orgId, long queueEntryId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PendingCheckInDto>> GetPendingCheckInsAsync(
        int orgId, DateOnly? queueDate, long? professionalId, long? branchId,
        string? search, int limit = 50, CancellationToken cancellationToken = default);

    Task<QueueListItemDto> CheckInAsync(
        int orgId, long createdBy, QueueCheckInRequest request, CancellationToken cancellationToken = default);

    Task<QueueListItemDto> AddWalkInAsync(
        int orgId, long createdBy, AddWalkInRequest request, CancellationToken cancellationToken = default);

    Task<QueueListItemDto> CallAsync(int orgId, long queueEntryId, CancellationToken cancellationToken = default);

    Task<QueueListItemDto> StartAsync(int orgId, long queueEntryId, CancellationToken cancellationToken = default);

    Task<QueueListItemDto> CompleteAsync(
        int orgId, long queueEntryId, long? updatedBy, CancellationToken cancellationToken = default);

    Task<QueueListItemDto> SkipAsync(
        int orgId, long queueEntryId, string? notes, CancellationToken cancellationToken = default);
}
