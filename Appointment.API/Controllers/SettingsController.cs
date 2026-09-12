using Appointment.API.Helpers;
using Appointment.Application.Services.Interfaces;
using Appointment.Domain.DTOs.Settings.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace Appointment.API.Controllers;

[Authorize]
[ApiController]
[Route("api/settings")]
public sealed class SettingsController : ControllerBase
{
    private readonly ISettingsService _settingsService;
    private readonly ILogger<SettingsController> _logger;

    public SettingsController(ISettingsService settingsService, ILogger<SettingsController> logger)
    {
        _settingsService = settingsService;
        _logger = logger;
    }

    /// <summary>GET api/settings/masters-summary</summary>
    [HttpGet("masters-summary")]
    public async Task<IActionResult> GetMastersSummaryAsync(CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            return Ok(await _settingsService.GetMastersSummaryAsync(orgId, cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Settings masters-summary failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/settings/masters-summary");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load masters summary." });
        }
    }

    /// <summary>GET api/settings/lookups?kind=&amp;includeInactive=</summary>
    [HttpGet("lookups")]
    public async Task<IActionResult> GetLookupsAsync(
        [FromQuery] string kind,
        [FromQuery] bool includeInactive = false,
        [FromQuery] int limit = 100,
        [FromQuery] int offset = 0,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _settingsService.GetLookupsAsync(
                orgId,
                new GetLookupsRequest
                {
                    LookupKind = kind,
                    IncludeInactive = includeInactive,
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
            _logger.LogWarning(ex, "Settings lookups failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/settings/lookups");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load lookups." });
        }
    }

    /// <summary>POST api/settings/lookups</summary>
    [HttpPost("lookups")]
    public async Task<IActionResult> CreateLookupAsync(
        [FromBody] SaveLookupRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var created = await _settingsService.CreateLookupAsync(orgId, request, cancellationToken);
            return Ok(created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Create lookup failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in POST api/settings/lookups");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to create lookup." });
        }
    }

    /// <summary>PUT api/settings/lookups/{lookupId}</summary>
    [HttpPut("lookups/{lookupId:int}")]
    public async Task<IActionResult> UpdateLookupAsync(
        int lookupId,
        [FromBody] UpdateLookupRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            return Ok(await _settingsService.UpdateLookupAsync(orgId, lookupId, request, cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Update lookup failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in PUT api/settings/lookups/{LookupId}", lookupId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to update lookup." });
        }
    }

    /// <summary>DELETE api/settings/lookups/{lookupId}</summary>
    [HttpDelete("lookups/{lookupId:int}")]
    public async Task<IActionResult> DeactivateLookupAsync(
        int lookupId,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            await _settingsService.DeactivateLookupAsync(orgId, lookupId, cancellationToken);
            return Ok(new { deleted = true, lookupId });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Deactivate lookup failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in DELETE api/settings/lookups/{LookupId}", lookupId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to delete lookup." });
        }
    }

    /// <summary>GET api/settings/appointment-rules</summary>
    [HttpGet("appointment-rules")]
    public async Task<IActionResult> GetAppointmentRulesAsync(CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            return Ok(await _settingsService.GetRulesAsync(orgId, cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Get appointment rules failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/settings/appointment-rules");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load appointment rules." });
        }
    }

    /// <summary>PUT api/settings/appointment-rules</summary>
    [HttpPut("appointment-rules")]
    public async Task<IActionResult> SaveAppointmentRulesAsync(
        [FromBody] SaveAppointmentRulesRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            return Ok(await _settingsService.SetRulesAsync(orgId, request, cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Save appointment rules failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in PUT api/settings/appointment-rules");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to save appointment rules." });
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
