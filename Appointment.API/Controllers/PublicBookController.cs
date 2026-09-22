using Appointment.API.Helpers;
using Appointment.Application.Services.Interfaces;
using Appointment.Domain.DTOs.PublicBook.Requests;
using Appointment.Domain.Exceptions;
using Appointment.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace Appointment.API.Controllers;

/// <summary>
/// Public QR self-booking APIs (no SoftOnCloud JWT).
/// Organisation is taken from encrypted booking token <c>t</c> — never trust a raw org id from the client.
/// Live DB: SoftOnCloud GET /api/auth/product-connection/service with X-Product-Service-Key
/// (orgId from <c>t</c>). Local: SoftOnCloud:UseProductConnectionDb=false → SecondConnection.
/// </summary>
[AllowAnonymous]
[ApiController]
[Route("api/public/book")]
public sealed class PublicBookController : ControllerBase
{
    private readonly IPublicBookingService _publicBookingService;
    private readonly IPublicBookTokenService _tokenService;
    private readonly IPublicBookOrgContext _publicBookOrg;
    private readonly ILogger<PublicBookController> _logger;

    public PublicBookController(
        IPublicBookingService publicBookingService,
        IPublicBookTokenService tokenService,
        IPublicBookOrgContext publicBookOrg,
        ILogger<PublicBookController> logger)
    {
        _publicBookingService = publicBookingService;
        _tokenService = tokenService;
        _publicBookOrg = publicBookOrg;
        _logger = logger;
    }

    [HttpGet("context")]
    public async Task<IActionResult> GetContextAsync(
        [FromQuery] string? t,
        CancellationToken cancellationToken = default)
    {
        if (!TryResolveOrg(t, out var orgId, out var error))
            return error!;

        try
        {
            var result = await _publicBookingService.GetContextAsync(orgId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Public book context failed");
            return StatusCode(500, new { message = "Unable to open booking page." });
        }
    }

    [HttpPost("customers")]
    public async Task<IActionResult> RegisterCustomerAsync(
        [FromBody] PublicCreateCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryResolveOrg(request?.Token, out var orgId, out var error))
            return error!;
        request!.OrgId = orgId;

        try
        {
            var result = await _publicBookingService.RegisterCustomerAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (CustomerDuplicateEmailException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Public register customer DB error");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Public register customer failed");
            return StatusCode(500, new { message = "Unable to save your details." });
        }
    }

    [HttpGet("customers/by-phone")]
    public async Task<IActionResult> FindByPhoneAsync(
        [FromQuery] string? t,
        [FromQuery] string phone,
        CancellationToken cancellationToken = default)
    {
        if (!TryResolveOrg(t, out var orgId, out var error))
            return error!;

        try
        {
            var result = await _publicBookingService.FindCustomerByPhoneAsync(orgId, phone, cancellationToken);
            if (result is null)
                return NotFound(new { message = "No customer found with this phone for this organisation." });
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Public find-by-phone DB error");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Public find-by-phone failed");
            return StatusCode(500, new { message = "Unable to look up customer." });
        }
    }

    [HttpGet("services")]
    public async Task<IActionResult> GetServicesAsync(
        [FromQuery] string? t,
        CancellationToken cancellationToken = default)
    {
        if (!TryResolveOrg(t, out var orgId, out var error))
            return error!;

        try
        {
            var result = await _publicBookingService.GetServicesAsync(orgId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Public services failed");
            return StatusCode(500, new { message = "Unable to load services." });
        }
    }

    [HttpGet("professionals")]
    public async Task<IActionResult> GetProfessionalsAsync(
        [FromQuery] string? t,
        [FromQuery] int productId,
        CancellationToken cancellationToken = default)
    {
        if (!TryResolveOrg(t, out var orgId, out var error))
            return error!;

        try
        {
            var result = await _publicBookingService.GetProfessionalsAsync(orgId, productId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Public professionals failed");
            return StatusCode(500, new { message = "Unable to load professionals." });
        }
    }

    [HttpGet("schedule-grid")]
    public async Task<IActionResult> GetScheduleGridAsync(
        [FromQuery] string? t,
        [FromQuery] int employeeId,
        [FromQuery] string from,
        [FromQuery] string to,
        CancellationToken cancellationToken = default)
    {
        if (!TryResolveOrg(t, out var orgId, out var error))
            return error!;

        try
        {
            if (!DateOnly.TryParse(from, out var fromDate) || !DateOnly.TryParse(to, out var toDate))
                return BadRequest(new { message = "from and to dates are required (YYYY-MM-DD)." });

            var result = await _publicBookingService.GetScheduleGridAsync(
                orgId, employeeId, fromDate, toDate, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Public schedule-grid failed");
            return StatusCode(500, new { message = "Unable to load available times." });
        }
    }

    [HttpGet("schedule")]
    public async Task<IActionResult> GetScheduleAsync(
        [FromQuery] string? t,
        [FromQuery] int employeeId,
        [FromQuery] string? asOfDate,
        CancellationToken cancellationToken = default)
    {
        if (!TryResolveOrg(t, out var orgId, out var error))
            return error!;

        try
        {
            DateOnly? asOf = null;
            if (!string.IsNullOrWhiteSpace(asOfDate))
            {
                if (!DateOnly.TryParse(asOfDate, out var parsed))
                    return BadRequest(new { message = "asOfDate must be YYYY-MM-DD." });
                asOf = parsed;
            }

            var result = await _publicBookingService.GetScheduleAsync(
                orgId, employeeId, asOf, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Public schedule failed");
            return StatusCode(500, new { message = "Unable to load professional schedule." });
        }
    }

    [HttpPost("appointments")]
    public async Task<IActionResult> CreateAppointmentAsync(
        [FromBody] PublicCreateAppointmentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryResolveOrg(request?.Token, out var orgId, out var error))
            return error!;
        request!.OrgId = orgId;

        try
        {
            var result = await _publicBookingService.CreateAppointmentAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Public create appointment DB error");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Public create appointment failed");
            return StatusCode(500, new { message = "Unable to book appointment." });
        }
    }

    private bool TryResolveOrg(string? token, out int orgId, out IActionResult? error)
    {
        orgId = 0;
        error = null;
        if (!_tokenService.TryResolve(token, out orgId))
        {
            error = BadRequest(new { message = "Invalid or missing booking link. Ask the clinic for a new QR code." });
            return false;
        }

        // So ProductDatabaseHelper can call SoftOnCloud product-connection/service for this org.
        _publicBookOrg.OrgId = orgId;
        return true;
    }
}
