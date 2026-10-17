using Appointment.API.Helpers;
using Appointment.Application.Services.Interfaces;
using Appointment.Domain.DTOs.FollowUps.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace Appointment.API.Controllers;

[Authorize]
[ApiController]
[Route("api/appointments/{appointmentId:long}/follow-ups")]
public sealed class FollowUpController : ControllerBase
{
    private readonly IFollowUpService _followUpService;
    private readonly ILogger<FollowUpController> _logger;

    public FollowUpController(IFollowUpService followUpService, ILogger<FollowUpController> logger)
    {
        _followUpService = followUpService;
        _logger = logger;
    }

    /// <summary>GET api/appointments/{id}/follow-ups</summary>
    [HttpGet]
    public async Task<IActionResult> ListAsync(long appointmentId, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out var userId, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });
        _ = userId;

        try
        {
            var result = await _followUpService.ListByParentAsync(orgId, appointmentId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "List follow-ups failed for appointment {AppointmentId}", appointmentId);
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled GET follow-ups for appointment {AppointmentId}", appointmentId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load follow-ups." });
        }
    }

    /// <summary>POST api/appointments/{id}/follow-ups — schedule one follow-up</summary>
    [HttpPost]
    public async Task<IActionResult> CreateOneAsync(
        long appointmentId,
        [FromBody] CreateFollowUpRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out var userId, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _followUpService.CreateOneAsync(
                orgId, appointmentId, userId, request, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Create follow-up failed for appointment {AppointmentId}", appointmentId);
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled POST follow-up for appointment {AppointmentId}", appointmentId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to schedule follow-up." });
        }
    }

    /// <summary>POST api/appointments/{id}/follow-ups/preview-series</summary>
    [HttpPost("preview-series")]
    public async Task<IActionResult> PreviewSeriesAsync(
        long appointmentId,
        [FromBody] CreateFollowUpSeriesRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out var userId, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });
        _ = userId;

        try
        {
            var result = await _followUpService.PreviewSeriesAsync(
                orgId, appointmentId, request, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Preview follow-up series failed for appointment {AppointmentId}", appointmentId);
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled preview-series for appointment {AppointmentId}", appointmentId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to preview series." });
        }
    }

    /// <summary>POST api/appointments/{id}/follow-ups/series</summary>
    [HttpPost("series")]
    public async Task<IActionResult> CreateSeriesAsync(
        long appointmentId,
        [FromBody] CreateFollowUpSeriesRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out var userId, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _followUpService.CreateSeriesAsync(
                orgId, appointmentId, userId, request, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Create follow-up series failed for appointment {AppointmentId}", appointmentId);
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled create series for appointment {AppointmentId}", appointmentId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to schedule series." });
        }
    }

    /// <summary>POST api/appointments/{id}/follow-ups/mark-done</summary>
    [HttpPost("mark-done")]
    public async Task<IActionResult> MarkPlanDoneAsync(
        long appointmentId, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out var userId, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _followUpService.MarkPlanDoneAsync(
                orgId, appointmentId, userId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Mark follow-up plan done failed for {AppointmentId}", appointmentId);
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled mark-done for appointment {AppointmentId}", appointmentId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to mark follow-up plan done." });
        }
    }

    /// <summary>POST api/appointments/{id}/follow-ups/mark-open</summary>
    [HttpPost("mark-open")]
    public async Task<IActionResult> MarkPlanOpenAsync(
        long appointmentId, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out var userId, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _followUpService.MarkPlanOpenAsync(
                orgId, appointmentId, userId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Mark follow-up plan open failed for {AppointmentId}", appointmentId);
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled mark-open for appointment {AppointmentId}", appointmentId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to reopen follow-up plan." });
        }
    }

    /// <summary>POST api/appointments/{id}/follow-ups/{childId}/cancel</summary>
    [HttpPost("{childId:long}/cancel")]
    public async Task<IActionResult> CancelChildAsync(
        long appointmentId,
        long childId,
        [FromBody] CancelFollowUpChildRequest? request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out var userId, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _followUpService.CancelChildAsync(
                orgId, appointmentId, childId, userId,
                request?.CancellationReason, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Cancel follow-up child {ChildId} failed under {AppointmentId}", childId, appointmentId);
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled cancel follow-up child {ChildId}", childId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to cancel follow-up." });
        }
    }

    private bool TryGetAuthContext(out long userId, out int orgId)
    {
        userId = 0;
        orgId = 0;
        var userClaim = User.FindFirst("user_id")?.Value;
        var orgClaim = User.FindFirst("org_id")?.Value;
        if (!long.TryParse(userClaim, out userId) || userId <= 0) return false;
        if (!int.TryParse(orgClaim, out orgId) || orgId <= 0) return false;
        return true;
    }
}
