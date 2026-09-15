using Appointment.API.Helpers;
using Appointment.Application.Services.Interfaces;
using Appointment.Domain.DTOs.Reminders.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace Appointment.API.Controllers;

[Authorize]
[ApiController]
[Route("api/reminders")]
public sealed class ReminderController : ControllerBase
{
    private readonly IReminderService _reminderService;
    private readonly ILogger<ReminderController> _logger;

    public ReminderController(IReminderService reminderService, ILogger<ReminderController> logger)
    {
        _reminderService = reminderService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetRemindersAsync(
        [FromQuery] string? search,
        [FromQuery] string? sendStatus,
        [FromQuery] string? referenceEntity,
        [FromQuery] long? appointmentId,
        [FromQuery] int limit = 50,
        [FromQuery] int offset = 0,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _reminderService.GetRemindersAsync(
                orgId,
                new GetRemindersRequest
                {
                    Search = search,
                    SendStatus = sendStatus,
                    ReferenceEntity = referenceEntity,
                    AppointmentId = appointmentId,
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
            _logger.LogWarning(ex, "Reminder list failed");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET api/reminders failed");
            return StatusCode(500, new { message = "Failed to load reminders." });
        }
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            return Ok(await _reminderService.GetStatsAsync(orgId, cancellationToken));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET api/reminders/stats failed");
            return StatusCode(500, new { message = "Failed to load reminder stats." });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateManualAsync(
        [FromBody] CreateManualReminderRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out var userId, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var created = await _reminderService.CreateManualAsync(orgId, userId, request, cancellationToken);
            return Ok(created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "POST api/reminders failed");
            return StatusCode(500, new { message = "Failed to create reminder." });
        }
    }

    [HttpPost("{notificationId:long}/cancel")]
    public async Task<IActionResult> CancelAsync(long notificationId, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            await _reminderService.CancelAsync(orgId, notificationId, cancellationToken);
            return Ok(new { cancelled = true, notificationId });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cancel reminder failed");
            return StatusCode(500, new { message = "Failed to cancel reminder." });
        }
    }

    /// <summary>Optional manual kick for smoke tests (same as worker).</summary>
    [HttpPost("process-due")]
    public async Task<IActionResult> ProcessDueAsync(CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out _))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            await _reminderService.ProcessDueAsync(cancellationToken);
            return Ok(new { processed = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "process-due failed");
            return StatusCode(500, new { message = "Failed to process due reminders." });
        }
    }

    private bool TryGetAuthContext(out long userId, out int orgId)
    {
        userId = 0;
        orgId = 0;
        var userIdRaw = User.FindFirst("user_id")?.Value;
        var orgIdRaw = User.FindFirst("org_id")?.Value;
        return long.TryParse(userIdRaw, out userId) && int.TryParse(orgIdRaw, out orgId) && userId > 0 && orgId > 0;
    }
}
