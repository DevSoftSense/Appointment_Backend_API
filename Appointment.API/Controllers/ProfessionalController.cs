using Appointment.API.Helpers;
using Appointment.Application.Services.Interfaces;
using Appointment.Domain.DTOs.Professionals.Requests;
using Appointment.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace Appointment.API.Controllers;

[Authorize]
[ApiController]
[Route("api/professionals")]
public sealed class ProfessionalController : ControllerBase
{
    private readonly IProfessionalService _professionalService;
    private readonly ILogger<ProfessionalController> _logger;

    public ProfessionalController(IProfessionalService professionalService, ILogger<ProfessionalController> logger)
    {
        _professionalService = professionalService;
        _logger = logger;
    }

    /// <summary>GET api/professionals?search=&amp;status=&amp;productId=&amp;limit=&amp;offset=</summary>
    [HttpGet]
    public async Task<IActionResult> GetProfessionalsAsync(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] int? productId,
        [FromQuery] int limit = 50,
        [FromQuery] int offset = 0,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _professionalService.GetProfessionalsAsync(
                orgId,
                new GetProfessionalsRequest
                {
                    Search = search,
                    Status = status,
                    ProductId = productId,
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
            _logger.LogWarning(ex, "Professional list failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/professionals");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load professionals." });
        }
    }

    /// <summary>GET api/professionals/stats</summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetProfessionalStatsAsync(CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _professionalService.GetProfessionalStatsAsync(orgId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Professional stats failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/professionals/stats");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load professional stats." });
        }
    }

    /// <summary>GET api/professionals/branches</summary>
    [HttpGet("branches")]
    public async Task<IActionResult> GetBranchesAsync(CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _professionalService.GetBranchesAsync(orgId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Branches failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/professionals/branches");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load branches." });
        }
    }

    /// <summary>GET api/professionals/available-services</summary>
    [HttpGet("available-services")]
    public async Task<IActionResult> GetAvailableServicesAsync(CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _professionalService.GetAvailableServicesAsync(orgId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Available services failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/professionals/available-services");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load services." });
        }
    }

    /// <summary>GET api/professionals/{employeeId}/services</summary>
    [HttpGet("{employeeId:int}/services")]
    public async Task<IActionResult> ListProfessionalServicesAsync(
        int employeeId,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _professionalService.ListProfessionalServicesAsync(orgId, employeeId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "List professional services failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/professionals/{EmployeeId}/services", employeeId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load professional services." });
        }
    }

    /// <summary>PUT api/professionals/{employeeId}/services — body: { "productIds": [1,2,3] }</summary>
    [HttpPut("{employeeId:int}/services")]
    public async Task<IActionResult> SetProfessionalServicesAsync(
        int employeeId,
        [FromBody] SetProfessionalServicesRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _professionalService.SetProfessionalServicesAsync(
                orgId,
                employeeId,
                request?.ProductIds ?? [],
                cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Set professional services failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in PUT api/professionals/{EmployeeId}/services", employeeId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to save professional services." });
        }
    }

    /// <summary>GET api/professionals/schedule-grid?from=&amp;to=&amp;employeeIds=1,2,3</summary>
    [HttpGet("schedule-grid")]
    public async Task<IActionResult> GetScheduleGridMultiAsync(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        [FromQuery] string? employeeIds,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var ids = ParseEmployeeIds(employeeIds);
            var result = await _professionalService.GetScheduleGridAsync(orgId, from, to, ids, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Get schedule grid (multi) failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/professionals/schedule-grid");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load schedule grid." });
        }
    }

    /// <summary>GET api/professionals/{employeeId}/schedule</summary>
    [HttpGet("{employeeId:int}/schedule")]
    public async Task<IActionResult> GetScheduleAsync(
        int employeeId,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _professionalService.GetScheduleAsync(orgId, employeeId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Get professional schedule failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/professionals/{EmployeeId}/schedule", employeeId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load professional schedule." });
        }
    }

    /// <summary>GET api/professionals/{employeeId}/schedule-grid?from=&amp;to=</summary>
    [HttpGet("{employeeId:int}/schedule-grid")]
    public async Task<IActionResult> GetScheduleGridAsync(
        int employeeId,
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _professionalService.GetScheduleGridAsync(
                orgId, from, to, [employeeId], cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Get professional schedule grid failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/professionals/{EmployeeId}/schedule-grid", employeeId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load schedule grid." });
        }
    }

    /// <summary>PUT api/professionals/{employeeId}/schedule</summary>
    [HttpPut("{employeeId:int}/schedule")]
    public async Task<IActionResult> SaveScheduleAsync(
        int employeeId,
        [FromBody] SaveProfessionalScheduleRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _professionalService.SaveScheduleAsync(orgId, employeeId, request, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Save professional schedule failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in PUT api/professionals/{EmployeeId}/schedule", employeeId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to save professional schedule." });
        }
    }

    /// <summary>GET api/professionals/departments?branchId=</summary>
    [HttpGet("departments")]
    public async Task<IActionResult> GetDepartmentsAsync(
        [FromQuery] int? branchId,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _professionalService.GetDepartmentsAsync(orgId, branchId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Departments failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/professionals/departments");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load departments." });
        }
    }

    /// <summary>GET api/professionals/roles?branchId=</summary>
    [HttpGet("roles")]
    public async Task<IActionResult> GetRolesAsync(
        [FromQuery] int? branchId,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _professionalService.GetRolesAsync(orgId, branchId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Roles failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/professionals/roles");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load roles." });
        }
    }

    /// <summary>GET api/professionals/{employeeId}</summary>
    [HttpGet("{employeeId:int}")]
    public async Task<IActionResult> GetProfessionalByIdAsync(
        int employeeId,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _professionalService.GetProfessionalByIdAsync(orgId, employeeId, cancellationToken);
            if (result is null)
                return NotFound(new { message = "Professional not found." });

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Get professional failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/professionals/{EmployeeId}", employeeId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load professional." });
        }
    }

    /// <summary>POST api/professionals</summary>
    [HttpPost]
    public async Task<IActionResult> CreateProfessionalAsync(
        [FromBody] CreateProfessionalRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        if (request is null)
            return BadRequest(new { message = "Request body is required." });

        try
        {
            var result = await _professionalService.CreateProfessionalAsync(orgId, request, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ProfessionalDuplicateEmailException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Create professional failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in POST api/professionals");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to create professional." });
        }
    }

    /// <summary>PUT api/professionals/{employeeId}</summary>
    [HttpPut("{employeeId:int}")]
    public async Task<IActionResult> UpdateProfessionalAsync(
        int employeeId,
        [FromBody] UpdateProfessionalRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        if (request is null)
            return BadRequest(new { message = "Request body is required." });

        try
        {
            var result = await _professionalService.UpdateProfessionalAsync(orgId, employeeId, request, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ProfessionalDuplicateEmailException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Update professional failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in PUT api/professionals/{EmployeeId}", employeeId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to update professional." });
        }
    }

    /// <summary>DELETE api/professionals/{employeeId} — soft delete (is_deleted=true; status unchanged)</summary>
    [HttpDelete("{employeeId:int}")]
    public async Task<IActionResult> DeactivateProfessionalAsync(
        int employeeId,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _professionalService.DeactivateProfessionalAsync(orgId, employeeId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Soft-delete professional failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in DELETE api/professionals/{EmployeeId}", employeeId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to soft-delete professional." });
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

    private static List<int> ParseEmployeeIds(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return [];

        return raw
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(part => int.TryParse(part, out var id) ? id : 0)
            .Where(id => id > 0)
            .Distinct()
            .ToList();
    }
}
