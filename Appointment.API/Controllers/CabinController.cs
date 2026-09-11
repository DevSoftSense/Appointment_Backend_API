using Appointment.API.Helpers;
using Appointment.Application.Services.Interfaces;
using Appointment.Domain.DTOs.Cabins.Requests;
using Appointment.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace Appointment.API.Controllers;

[Authorize]
[ApiController]
[Route("api/cabins")]
public sealed class CabinController : ControllerBase
{
    private readonly ICabinService _cabinService;
    private readonly ILogger<CabinController> _logger;

    public CabinController(ICabinService cabinService, ILogger<CabinController> logger)
    {
        _cabinService = cabinService;
        _logger = logger;
    }

    /// <summary>GET api/cabins?search=&amp;isActive=&amp;branchId=&amp;limit=&amp;offset=</summary>
    [HttpGet]
    public async Task<IActionResult> GetCabinsAsync(
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        [FromQuery] int? branchId,
        [FromQuery] int limit = 50,
        [FromQuery] int offset = 0,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _cabinService.GetCabinsAsync(
                orgId,
                new GetCabinsRequest
                {
                    Search = search,
                    IsActive = isActive,
                    BranchId = branchId,
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
            _logger.LogWarning(ex, "Cabin list failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/cabins");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load cabins." });
        }
    }

    /// <summary>GET api/cabins/stats</summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _cabinService.GetStatsAsync(orgId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Cabin stats failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/cabins/stats");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load cabin stats." });
        }
    }

    /// <summary>GET api/cabins/branches</summary>
    [HttpGet("branches")]
    public async Task<IActionResult> GetBranchesAsync(CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _cabinService.GetBranchesAsync(orgId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Cabin branches failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/cabins/branches");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load branches." });
        }
    }

    /// <summary>GET api/cabins/{cabinResourceId}</summary>
    [HttpGet("{cabinResourceId:long}")]
    public async Task<IActionResult> GetCabinByIdAsync(
        long cabinResourceId,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _cabinService.GetCabinByIdAsync(orgId, cabinResourceId, cancellationToken);
            if (result is null)
                return NotFound(new { message = "Cabin / room not found." });

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Get cabin failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/cabins/{CabinResourceId}", cabinResourceId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load cabin." });
        }
    }

    /// <summary>POST api/cabins</summary>
    [HttpPost]
    public async Task<IActionResult> CreateCabinAsync(
        [FromBody] CreateCabinRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        if (request is null)
            return BadRequest(new { message = "Request body is required." });

        try
        {
            var result = await _cabinService.CreateCabinAsync(orgId, request, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (CabinDuplicateException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Create cabin failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in POST api/cabins");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to create cabin." });
        }
    }

    /// <summary>PUT api/cabins/{cabinResourceId}</summary>
    [HttpPut("{cabinResourceId:long}")]
    public async Task<IActionResult> UpdateCabinAsync(
        long cabinResourceId,
        [FromBody] UpdateCabinRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        if (request is null)
            return BadRequest(new { message = "Request body is required." });

        try
        {
            var result = await _cabinService.UpdateCabinAsync(orgId, cabinResourceId, request, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (CabinDuplicateException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Update cabin failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in PUT api/cabins/{CabinResourceId}", cabinResourceId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to update cabin." });
        }
    }

    /// <summary>DELETE api/cabins/{cabinResourceId} — sets is_active false</summary>
    [HttpDelete("{cabinResourceId:long}")]
    public async Task<IActionResult> DeactivateCabinAsync(
        long cabinResourceId,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _cabinService.DeactivateCabinAsync(orgId, cabinResourceId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Deactivate cabin failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in DELETE api/cabins/{CabinResourceId}", cabinResourceId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to deactivate cabin." });
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
