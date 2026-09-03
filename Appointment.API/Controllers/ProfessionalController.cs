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

    /// <summary>GET api/professionals?search=&amp;status=&amp;limit=&amp;offset=</summary>
    [HttpGet]
    public async Task<IActionResult> GetProfessionalsAsync(
        [FromQuery] string? search,
        [FromQuery] string? status,
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
