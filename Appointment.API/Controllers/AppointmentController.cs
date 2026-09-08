using Appointment.API.Helpers;
using Appointment.Application.Services.Interfaces;
using Appointment.Domain.DTOs.Appointments.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace Appointment.API.Controllers;

[Authorize]
[ApiController]
[Route("api/appointments")]
public sealed class AppointmentController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;
    private readonly IAppointmentDocumentService _documentService;
    private readonly ILogger<AppointmentController> _logger;

    public AppointmentController(
        IAppointmentService appointmentService,
        IAppointmentDocumentService documentService,
        ILogger<AppointmentController> logger)
    {
        _appointmentService = appointmentService;
        _documentService = documentService;
        _logger = logger;
    }

    /// <summary>GET api/appointments</summary>
    [HttpGet]
    public async Task<IActionResult> GetAppointmentsAsync(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] DateOnly? dateFrom,
        [FromQuery] DateOnly? dateTo,
        [FromQuery] long? customerId,
        [FromQuery] long? professionalId,
        [FromQuery] long? productId,
        [FromQuery] long? branchId,
        [FromQuery] string? source,
        [FromQuery] string? appointmentType,
        [FromQuery] int limit = 50,
        [FromQuery] int offset = 0,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out var userId, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        _ = userId;

        try
        {
            var result = await _appointmentService.GetAppointmentsAsync(
                orgId,
                new GetAppointmentsRequest
                {
                    Search = search,
                    Status = status,
                    DateFrom = dateFrom,
                    DateTo = dateTo,
                    CustomerId = customerId,
                    ProfessionalId = professionalId,
                    ProductId = productId,
                    BranchId = branchId,
                    Source = source,
                    AppointmentType = appointmentType,
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
            _logger.LogWarning(ex, "Appointment list failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/appointments");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load appointments." });
        }
    }

    /// <summary>GET api/appointments/stats</summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStatsAsync(
        [FromQuery] long? customerId,
        [FromQuery] long? professionalId,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _appointmentService.GetAppointmentStatsAsync(
                orgId, customerId, professionalId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Appointment stats failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/appointments/stats");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load appointment stats." });
        }
    }

    /// <summary>GET api/appointments/{appointmentId}</summary>
    [HttpGet("{appointmentId:long}")]
    public async Task<IActionResult> GetByIdAsync(
        long appointmentId,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _appointmentService.GetAppointmentByIdAsync(orgId, appointmentId, cancellationToken);
            if (result is null)
                return NotFound(new { message = "Appointment not found." });

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Get appointment failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/appointments/{AppointmentId}", appointmentId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load appointment." });
        }
    }

    /// <summary>POST api/appointments</summary>
    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateAppointmentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out var userId, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        if (request is null)
            return BadRequest(new { message = "Request body is required." });

        try
        {
            var result = await _appointmentService.CreateAppointmentAsync(orgId, userId, request, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Create appointment failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in POST api/appointments");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to create appointment." });
        }
    }

    /// <summary>PUT api/appointments/{appointmentId}</summary>
    [HttpPut("{appointmentId:long}")]
    public async Task<IActionResult> UpdateAsync(
        long appointmentId,
        [FromBody] UpdateAppointmentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out var userId, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        if (request is null)
            return BadRequest(new { message = "Request body is required." });

        try
        {
            var result = await _appointmentService.UpdateAppointmentAsync(
                orgId, appointmentId, userId, request, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Update appointment failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in PUT api/appointments/{AppointmentId}", appointmentId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to update appointment." });
        }
    }

    /// <summary>POST api/appointments/{appointmentId}/cancel</summary>
    [HttpPost("{appointmentId:long}/cancel")]
    public async Task<IActionResult> CancelAsync(
        long appointmentId,
        [FromBody] CancelAppointmentRequest? request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out var userId, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _appointmentService.CancelAppointmentAsync(
                orgId, appointmentId, userId, request ?? new CancelAppointmentRequest(), cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Cancel appointment failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in POST api/appointments/{AppointmentId}/cancel", appointmentId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to cancel appointment." });
        }
    }

    /// <summary>POST api/appointments/{appointmentId}/check-in</summary>
    [HttpPost("{appointmentId:long}/check-in")]
    public async Task<IActionResult> CheckInAsync(
        long appointmentId,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out var userId, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _appointmentService.CheckInAppointmentAsync(
                orgId, appointmentId, userId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Check-in failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in POST api/appointments/{AppointmentId}/check-in", appointmentId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to check in." });
        }
    }

    /// <summary>POST api/appointments/{appointmentId}/complete</summary>
    [HttpPost("{appointmentId:long}/complete")]
    public async Task<IActionResult> CompleteAsync(
        long appointmentId,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out var userId, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _appointmentService.CompleteAppointmentAsync(
                orgId, appointmentId, userId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Complete failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in POST api/appointments/{AppointmentId}/complete", appointmentId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to complete appointment." });
        }
    }

    /// <summary>GET api/appointments/{appointmentId}/documents</summary>
    [HttpGet("{appointmentId:long}/documents")]
    public async Task<IActionResult> ListDocumentsAsync(
        long appointmentId,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var items = await _documentService.ListAsync(orgId, appointmentId, cancellationToken);
            return Ok(items);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Document list failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET documents for appointment {AppointmentId}", appointmentId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load documents." });
        }
    }

    /// <summary>POST api/appointments/{appointmentId}/documents — multipart form: file, optional documentType</summary>
    [HttpPost("{appointmentId:long}/documents")]
    [RequestSizeLimit(3 * 1024 * 1024)]
    public async Task<IActionResult> UploadDocumentAsync(
        long appointmentId,
        IFormFile? file,
        [FromForm] string? documentType,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out var userId, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var created = await _documentService.UploadAsync(
                orgId, userId, appointmentId, file!, documentType, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Document upload failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in POST documents for appointment {AppointmentId}", appointmentId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to upload document." });
        }
    }

    /// <summary>DELETE api/appointments/{appointmentId}/documents/{documentId}</summary>
    [HttpDelete("{appointmentId:long}/documents/{documentId:long}")]
    public async Task<IActionResult> DeleteDocumentAsync(
        long appointmentId,
        long documentId,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            await _documentService.DeleteAsync(orgId, appointmentId, documentId, cancellationToken);
            return Ok(new { deleted = true, documentId });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Document delete failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in DELETE document {DocumentId}", documentId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to delete document." });
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
