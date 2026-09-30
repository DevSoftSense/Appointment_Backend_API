using Appointment.API.Helpers;
using Appointment.Application.Services.Interfaces;
using Appointment.Domain.DTOs.Referrals.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace Appointment.API.Controllers;

[Authorize]
[ApiController]
[Route("api")]
public sealed class ReferralController : ControllerBase
{
    private readonly IReferralService _referralService;
    private readonly ILogger<ReferralController> _logger;

    public ReferralController(IReferralService referralService, ILogger<ReferralController> logger)
    {
        _referralService = referralService;
        _logger = logger;
    }

    /// <summary>GET api/customers/{accountId}/referral</summary>
    [HttpGet("customers/{accountId:long}/referral")]
    public async Task<IActionResult> GetCustomerReferralAsync(
        long accountId, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _referralService.GetCustomerDefaultAsync(orgId, accountId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Get customer referral failed.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error GET customers/{AccountId}/referral", accountId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load referral." });
        }
    }

    /// <summary>PUT api/customers/{accountId}/referral</summary>
    [HttpPut("customers/{accountId:long}/referral")]
    public async Task<IActionResult> UpsertCustomerReferralAsync(
        long accountId, [FromBody] UpsertReferralRequest request, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out var userId, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _referralService.UpsertCustomerDefaultAsync(
                orgId, accountId, userId, request, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Upsert customer referral failed.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error PUT customers/{AccountId}/referral", accountId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to save referral." });
        }
    }

    /// <summary>DELETE api/customers/{accountId}/referral</summary>
    [HttpDelete("customers/{accountId:long}/referral")]
    public async Task<IActionResult> ClearCustomerReferralAsync(
        long accountId, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out var userId, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            await _referralService.ClearCustomerDefaultAsync(orgId, accountId, userId, cancellationToken);
            return Ok(new { cleared = true });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Clear customer referral failed.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error DELETE customers/{AccountId}/referral", accountId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to clear referral." });
        }
    }

    /// <summary>GET api/appointments/{appointmentId}/referral</summary>
    [HttpGet("appointments/{appointmentId:long}/referral")]
    public async Task<IActionResult> GetAppointmentReferralAsync(
        long appointmentId, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _referralService.GetByAppointmentAsync(orgId, appointmentId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Get appointment referral failed.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error GET appointments/{AppointmentId}/referral", appointmentId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load referral." });
        }
    }

    /// <summary>PUT api/appointments/{appointmentId}/referral</summary>
    [HttpPut("appointments/{appointmentId:long}/referral")]
    public async Task<IActionResult> UpsertAppointmentReferralAsync(
        long appointmentId, [FromBody] UpsertReferralRequest request, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out var userId, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _referralService.UpsertForAppointmentAsync(
                orgId, appointmentId, userId, request, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Upsert appointment referral failed.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error PUT appointments/{AppointmentId}/referral", appointmentId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to save referral." });
        }
    }

    /// <summary>DELETE api/appointments/{appointmentId}/referral</summary>
    [HttpDelete("appointments/{appointmentId:long}/referral")]
    public async Task<IActionResult> ClearAppointmentReferralAsync(
        long appointmentId, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out var userId, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            await _referralService.ClearForAppointmentAsync(orgId, appointmentId, userId, cancellationToken);
            return Ok(new { cleared = true });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Clear appointment referral failed.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error DELETE appointments/{AppointmentId}/referral", appointmentId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to clear referral." });
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
