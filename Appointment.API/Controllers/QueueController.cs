using Appointment.API.Helpers;
using Appointment.Application.Services.Interfaces;
using Appointment.Domain.DTOs.Queue.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace Appointment.API.Controllers;

[Authorize]
[ApiController]
[Route("api/queue")]
public sealed class QueueController : ControllerBase
{
    private readonly IQueueService _queueService;
    private readonly ILogger<QueueController> _logger;

    public QueueController(IQueueService queueService, ILogger<QueueController> logger)
    {
        _queueService = queueService;
        _logger = logger;
    }

    /// <summary>GET api/queue</summary>
    [HttpGet]
    public async Task<IActionResult> GetQueueAsync(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] DateOnly? queueDate,
        [FromQuery] long? customerId,
        [FromQuery] long? professionalId,
        [FromQuery] long? productId,
        [FromQuery] long? branchId,
        [FromQuery] int limit = 100,
        [FromQuery] int offset = 0,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _queueService.GetQueueAsync(
                orgId,
                new GetQueueRequest
                {
                    Search = search,
                    Status = status,
                    QueueDate = queueDate,
                    CustomerId = customerId,
                    ProfessionalId = professionalId,
                    ProductId = productId,
                    BranchId = branchId,
                    Limit = limit,
                    Offset = offset
                },
                cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Queue list failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/queue");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load queue." });
        }
    }

    /// <summary>GET api/queue/stats</summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStatsAsync(
        [FromQuery] DateOnly? queueDate,
        [FromQuery] long? professionalId,
        [FromQuery] long? branchId,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _queueService.GetStatsAsync(
                orgId, queueDate, professionalId, branchId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Queue stats failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/queue/stats");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load queue stats." });
        }
    }

    /// <summary>GET api/queue/pending-checkins</summary>
    [HttpGet("pending-checkins")]
    public async Task<IActionResult> GetPendingCheckInsAsync(
        [FromQuery] DateOnly? queueDate,
        [FromQuery] long? professionalId,
        [FromQuery] long? branchId,
        [FromQuery] string? search,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _queueService.GetPendingCheckInsAsync(
                orgId, queueDate, professionalId, branchId, search, limit, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Pending check-ins failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/queue/pending-checkins");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load pending check-ins." });
        }
    }

    /// <summary>GET api/queue/{queueEntryId}</summary>
    [HttpGet("{queueEntryId:long}")]
    public async Task<IActionResult> GetByIdAsync(
        long queueEntryId, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _queueService.GetByIdAsync(orgId, queueEntryId, cancellationToken);
            if (result is null)
                return NotFound(new { message = "Queue entry not found." });
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Get queue entry failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/queue/{QueueEntryId}", queueEntryId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load queue entry." });
        }
    }

    /// <summary>POST api/queue/check-in — booked patient arrives</summary>
    [HttpPost("check-in")]
    public async Task<IActionResult> CheckInAsync(
        [FromBody] QueueCheckInRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out var userId, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        if (request is null)
            return BadRequest(new { message = "Request body is required." });

        try
        {
            var result = await _queueService.CheckInAsync(orgId, userId, request, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Queue check-in failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in POST api/queue/check-in");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to check in." });
        }
    }

    /// <summary>POST api/queue/walk-in — no prior booking</summary>
    [HttpPost("walk-in")]
    public async Task<IActionResult> AddWalkInAsync(
        [FromBody] AddWalkInRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out var userId, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        if (request is null)
            return BadRequest(new { message = "Request body is required." });

        try
        {
            var result = await _queueService.AddWalkInAsync(orgId, userId, request, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Walk-in failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in POST api/queue/walk-in");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to add walk-in." });
        }
    }

    /// <summary>POST api/queue/{queueEntryId}/call</summary>
    [HttpPost("{queueEntryId:long}/call")]
    public async Task<IActionResult> CallAsync(
        long queueEntryId, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _queueService.CallAsync(orgId, queueEntryId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Queue call failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in POST api/queue/{QueueEntryId}/call", queueEntryId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to call patient." });
        }
    }

    /// <summary>POST api/queue/{queueEntryId}/start</summary>
    [HttpPost("{queueEntryId:long}/start")]
    public async Task<IActionResult> StartAsync(
        long queueEntryId, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _queueService.StartAsync(orgId, queueEntryId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Queue start failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in POST api/queue/{QueueEntryId}/start", queueEntryId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to start service." });
        }
    }

    /// <summary>POST api/queue/{queueEntryId}/complete</summary>
    [HttpPost("{queueEntryId:long}/complete")]
    public async Task<IActionResult> CompleteAsync(
        long queueEntryId, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out var userId, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _queueService.CompleteAsync(orgId, queueEntryId, userId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Queue complete failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in POST api/queue/{QueueEntryId}/complete", queueEntryId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to complete queue entry." });
        }
    }

    /// <summary>POST api/queue/{queueEntryId}/skip</summary>
    [HttpPost("{queueEntryId:long}/skip")]
    public async Task<IActionResult> SkipAsync(
        long queueEntryId,
        [FromBody] QueueNotesRequest? request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _queueService.SkipAsync(
                orgId, queueEntryId, request?.Notes, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Queue skip failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in POST api/queue/{QueueEntryId}/skip", queueEntryId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to skip queue entry." });
        }
    }

    private bool TryGetAuthContext(out int userId, out int orgId)
    {
        userId = 0;
        orgId = 0;

        var userIdRaw = User.FindFirst("user_id")?.Value;
        var orgIdRaw = User.FindFirst("org_id")?.Value;

        return int.TryParse(userIdRaw, out userId)
               && userId > 0
               && int.TryParse(orgIdRaw, out orgId)
               && orgId > 0;
    }
}
