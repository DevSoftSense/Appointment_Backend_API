using Appointment.Application.Services.Interfaces;
using Appointment.Domain.DTOs.Queue.Requests;
using Appointment.Domain.DTOs.Queue.Responses;
using Appointment.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Appointment.Application.Services.Classes;

public sealed class QueueService : IQueueService
{
    private readonly IQueueRepository _queueRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<QueueService> _logger;

    public QueueService(
        IQueueRepository queueRepository,
        IConfiguration configuration,
        ILogger<QueueService> logger)
    {
        _queueRepository = queueRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<QueueListResponse> GetQueueAsync(
        int orgId, GetQueueRequest request, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        request ??= new GetQueueRequest();
        if (request.Limit <= 0) request.Limit = 100;
        if (request.Offset < 0) request.Offset = 0;

        var appId = GetAppId();
        var items = await _queueRepository.GetQueueAsync(orgId, appId, request, cancellationToken);

        return new QueueListResponse
        {
            Items = items,
            TotalCount = items.Count > 0 ? items[0].TotalCount : 0
        };
    }

    public async Task<QueueStatsDto> GetStatsAsync(
        int orgId, DateOnly? queueDate, long? professionalId, long? branchId,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        return await _queueRepository.GetStatsAsync(
            orgId, GetAppId(), queueDate, professionalId, branchId, cancellationToken);
    }

    public async Task<QueueListItemDto?> GetByIdAsync(
        int orgId, long queueEntryId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (queueEntryId <= 0)
            throw new ArgumentException("Queue entry id is required.", nameof(queueEntryId));

        return await _queueRepository.GetByIdAsync(orgId, GetAppId(), queueEntryId, cancellationToken);
    }

    public async Task<IReadOnlyList<PendingCheckInDto>> GetPendingCheckInsAsync(
        int orgId, DateOnly? queueDate, long? professionalId, long? branchId,
        string? search, int limit = 50, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (limit <= 0) limit = 50;

        return await _queueRepository.GetPendingCheckInsAsync(
            orgId, GetAppId(), queueDate, professionalId, branchId, search, limit, cancellationToken);
    }

    public async Task<QueueListItemDto> CheckInAsync(
        int orgId, long createdBy, QueueCheckInRequest request, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (request is null) throw new ArgumentNullException(nameof(request));
        if (createdBy <= 0) throw new ArgumentException("Created-by user is required.");
        if (request.AppointmentId <= 0) throw new ArgumentException("Appointment is required.");

        _logger.LogInformation(
            "Queue check-in org={OrgId} appointment={AppointmentId}",
            orgId, request.AppointmentId);

        return await _queueRepository.CheckInAsync(orgId, GetAppId(), createdBy, request, cancellationToken);
    }

    public async Task<QueueListItemDto> AddWalkInAsync(
        int orgId, long createdBy, AddWalkInRequest request, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (request is null) throw new ArgumentNullException(nameof(request));
        if (createdBy <= 0) throw new ArgumentException("Created-by user is required.");
        if (request.CustomerId <= 0) throw new ArgumentException("Customer is required.");
        if (request.ProfessionalId <= 0) throw new ArgumentException("Professional is required.");
        if (request.ProductId <= 0) throw new ArgumentException("Service is required.");

        _logger.LogInformation(
            "Queue walk-in org={OrgId} customer={CustomerId} professional={ProfessionalId}",
            orgId, request.CustomerId, request.ProfessionalId);

        return await _queueRepository.AddWalkInAsync(orgId, GetAppId(), createdBy, request, cancellationToken);
    }

    public Task<QueueListItemDto> CallAsync(
        int orgId, long queueEntryId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (queueEntryId <= 0) throw new ArgumentException("Queue entry id is required.");
        return _queueRepository.CallAsync(orgId, GetAppId(), queueEntryId, cancellationToken);
    }

    public Task<QueueListItemDto> StartAsync(
        int orgId, long queueEntryId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (queueEntryId <= 0) throw new ArgumentException("Queue entry id is required.");
        return _queueRepository.StartAsync(orgId, GetAppId(), queueEntryId, cancellationToken);
    }

    public Task<QueueListItemDto> CompleteAsync(
        int orgId, long queueEntryId, long? updatedBy, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (queueEntryId <= 0) throw new ArgumentException("Queue entry id is required.");
        return _queueRepository.CompleteAsync(orgId, GetAppId(), queueEntryId, updatedBy, cancellationToken);
    }

    public Task<QueueListItemDto> SkipAsync(
        int orgId, long queueEntryId, string? notes, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (queueEntryId <= 0) throw new ArgumentException("Queue entry id is required.");
        return _queueRepository.SkipAsync(orgId, GetAppId(), queueEntryId, notes, cancellationToken);
    }

    private void ValidateOrg(int orgId)
    {
        if (orgId <= 0)
            throw new ArgumentException("Organization id is required.", nameof(orgId));
    }

    private int GetAppId()
    {
        var raw = _configuration["Appointment:AppId"]
                  ?? _configuration["Appointment:ProductId"]
                  ?? "25";
        if (!int.TryParse(raw, out var appId) || appId <= 0)
            throw new InvalidOperationException("Appointment:AppId is not configured.");
        return appId;
    }
}
