using Appointment.API.Helpers;
using Appointment.Application.Services.Interfaces;
using Appointment.Domain.DTOs.Payments.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace Appointment.API.Controllers;

[Authorize]
[ApiController]
[Route("api/appointments/{appointmentId:long}/payment")]
public sealed class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(IPaymentService paymentService, ILogger<PaymentController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    /// <summary>GET api/appointments/{appointmentId}/payment</summary>
    [HttpGet]
    public async Task<IActionResult> GetAsync(long appointmentId, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _paymentService.GetByAppointmentAsync(orgId, appointmentId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Get payment failed for appointment {AppointmentId}", appointmentId);
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled GET payment for appointment {AppointmentId}", appointmentId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load payment." });
        }
    }

    /// <summary>POST api/appointments/{appointmentId}/payment — create bill (+ optional first receipt)</summary>
    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        long appointmentId, [FromBody] RecordPaymentRequest request, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out var userId, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _paymentService.CreateAsync(orgId, appointmentId, userId, request, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Create payment failed for appointment {AppointmentId}", appointmentId);
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled POST payment for appointment {AppointmentId}", appointmentId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to record payment." });
        }
    }

    /// <summary>PUT api/appointments/{appointmentId}/payment — replace bill lines</summary>
    [HttpPut]
    public async Task<IActionResult> UpdateAsync(
        long appointmentId, [FromBody] RecordPaymentRequest request, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out var userId, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _paymentService.UpdateBillAsync(orgId, appointmentId, userId, request, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Update payment failed for appointment {AppointmentId}", appointmentId);
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled PUT payment for appointment {AppointmentId}", appointmentId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to update bill." });
        }
    }

    /// <summary>POST api/appointments/{appointmentId}/payment/receipts — add receipt (partial → paid)</summary>
    [HttpPost("receipts")]
    public async Task<IActionResult> AddReceiptAsync(
        long appointmentId, [FromBody] AddPaymentReceiptRequest request, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out var userId, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _paymentService.AddPaymentAsync(orgId, appointmentId, userId, request, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Add receipt failed for appointment {AppointmentId}", appointmentId);
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled POST payment/receipts for appointment {AppointmentId}", appointmentId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to add payment." });
        }
    }

    /// <summary>DELETE api/appointments/{appointmentId}/payment — void bill</summary>
    [HttpDelete]
    public async Task<IActionResult> VoidAsync(long appointmentId, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out var userId, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _paymentService.VoidAsync(orgId, appointmentId, userId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Void payment failed for appointment {AppointmentId}", appointmentId);
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled DELETE payment for appointment {AppointmentId}", appointmentId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to void payment." });
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
