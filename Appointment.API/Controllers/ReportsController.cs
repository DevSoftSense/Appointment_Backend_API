using Appointment.API.Helpers;
using Appointment.Application.Services.Interfaces;
using Appointment.Domain.DTOs.Reports.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace Appointment.API.Controllers;

[Authorize]
[ApiController]
[Route("api/reports")]
public sealed class ReportsController : ControllerBase
{
    private readonly IReportsService _reportsService;
    private readonly IReportScheduleService _scheduleService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(
        IReportsService reportsService,
        IReportScheduleService scheduleService,
        ILogger<ReportsController> logger)
    {
        _reportsService = reportsService;
        _scheduleService = scheduleService;
        _logger = logger;
    }

    [HttpGet("overview")]
    public async Task<IActionResult> GetOverviewAsync(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        [FromQuery] int? branchId,
        [FromQuery] int? professionalId,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _reportsService.GetOverviewAsync(
                orgId, from, to, branchId, professionalId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Reports overview failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/reports/overview");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load report overview." });
        }
    }

    [HttpGet("appointments")]
    public async Task<IActionResult> GetAppointmentsAsync(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        [FromQuery] int? branchId,
        [FromQuery] int? professionalId,
        [FromQuery] string? status,
        [FromQuery] int limit = 100,
        [FromQuery] int offset = 0,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _reportsService.GetAppointmentsAsync(
                orgId, from, to, branchId, professionalId, status, limit, offset, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Reports appointments failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/reports/appointments");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load appointments report." });
        }
    }

    [HttpGet("services")]
    public async Task<IActionResult> GetServicesAsync(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        [FromQuery] int? branchId,
        [FromQuery] int? professionalId,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _reportsService.GetServicesAsync(
                orgId, from, to, branchId, professionalId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Reports services failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/reports/services");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load services report." });
        }
    }

    [HttpGet("professionals")]
    public async Task<IActionResult> GetProfessionalsAsync(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        [FromQuery] int? branchId,
        [FromQuery] int? professionalId,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _reportsService.GetProfessionalsAsync(
                orgId, from, to, branchId, professionalId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Reports professionals failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/reports/professionals");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load professionals report." });
        }
    }

    [HttpGet("no-show")]
    public async Task<IActionResult> GetNoShowAsync(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        [FromQuery] int? branchId,
        [FromQuery] int? professionalId,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _reportsService.GetNoShowAsync(
                orgId, from, to, branchId, professionalId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Reports no-show failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/reports/no-show");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load no-show report." });
        }
    }

    [HttpGet("customers")]
    public async Task<IActionResult> GetCustomersAsync(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        [FromQuery] int? branchId,
        [FromQuery] int? professionalId,
        [FromQuery] int limit = 100,
        [FromQuery] int offset = 0,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _reportsService.GetCustomersAsync(
                orgId, from, to, branchId, professionalId, limit, offset, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Reports customers failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/reports/customers");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load customers report." });
        }
    }

    [HttpGet("schedules")]
    public async Task<IActionResult> ListSchedulesAsync(
        [FromQuery] string? sendStatus,
        [FromQuery] int limit = 50,
        [FromQuery] int offset = 0,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _scheduleService.ListAsync(
                orgId,
                new GetReportSchedulesRequest { SendStatus = sendStatus, Limit = limit, Offset = offset },
                cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "List report schedules failed in DB.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/reports/schedules");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to list scheduled reports." });
        }
    }

    [HttpPost("schedules")]
    public async Task<IActionResult> CreateScheduleAsync(
        [FromBody] CreateReportScheduleRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out var userId, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _scheduleService.CreateAsync(orgId, userId, request, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Create report schedule failed in DB.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in POST api/reports/schedules");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to create scheduled report." });
        }
    }

    [HttpDelete("schedules/{notificationId:long}")]
    public async Task<IActionResult> CancelScheduleAsync(
        long notificationId,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            await _scheduleService.CancelAsync(orgId, notificationId, cancellationToken);
            return Ok(new { cancelled = true, notificationId });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Cancel report schedule failed in DB.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in DELETE api/reports/schedules/{Id}", notificationId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to cancel scheduled report." });
        }
    }

    [HttpPost("schedules/process-due")]
    public async Task<IActionResult> ProcessDueSchedulesAsync(CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out _))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            await _scheduleService.ProcessDueAsync(cancellationToken);
            return Ok(new { processed = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in POST api/reports/schedules/process-due");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to process due schedules." });
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
