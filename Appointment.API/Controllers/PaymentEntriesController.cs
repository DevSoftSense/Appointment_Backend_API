using Appointment.API.Helpers;
using Appointment.Application.Services.Interfaces;
using Appointment.Domain.DTOs.Payments.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace Appointment.API.Controllers;

/// <summary>Org-wide Appointment payment entries list (ops inbox — not SoftOnCloud Payments product).</summary>
[Authorize]
[ApiController]
[Route("api/payments")]
public sealed class PaymentEntriesController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentEntriesController> _logger;

    public PaymentEntriesController(IPaymentService paymentService, ILogger<PaymentEntriesController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    /// <summary>GET api/payments</summary>
    [HttpGet]
    public async Task<IActionResult> ListAsync(
        [FromQuery] DateOnly? fromDate = null,
        [FromQuery] DateOnly? toDate = null,
        [FromQuery] string? paymentStatus = null,
        [FromQuery] long? branchId = null,
        [FromQuery] string? search = null,
        [FromQuery] int limit = 50,
        [FromQuery] int offset = 0,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _paymentService.ListAsync(orgId, new GetPaymentEntriesRequest
            {
                FromDate = fromDate,
                ToDate = toDate,
                PaymentStatus = paymentStatus,
                BranchId = branchId,
                Search = search,
                Limit = limit,
                Offset = offset
            }, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Payment entries list failed");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled GET api/payments");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load payment entries." });
        }
    }

    /// <summary>GET api/payments/stats</summary>
    [HttpGet("stats")]
    public async Task<IActionResult> StatsAsync(
        [FromQuery] DateOnly? fromDate = null,
        [FromQuery] DateOnly? toDate = null,
        [FromQuery] long? branchId = null,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _paymentService.ListStatsAsync(orgId, new GetPaymentEntriesRequest
            {
                FromDate = fromDate,
                ToDate = toDate,
                BranchId = branchId
            }, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Payment entries stats failed");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled GET api/payments/stats");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load payment stats." });
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
