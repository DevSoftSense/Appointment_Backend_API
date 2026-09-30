using Appointment.API.Helpers;
using Appointment.Application.Services.Interfaces;
using Appointment.Domain.DTOs.Permissions.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace Appointment.API.Controllers;

[Authorize]
[ApiController]
[Route("api/permissions")]
public sealed class PermissionController : ControllerBase
{
    private readonly IPermissionService _permissionService;
    private readonly ILogger<PermissionController> _logger;

    public PermissionController(
        IPermissionService permissionService,
        ILogger<PermissionController> logger)
    {
        _permissionService = permissionService;
        _logger = logger;
    }

    /// <summary>GET api/permissions/me?roleCodes=A,B</summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMyAsync(
        [FromQuery] string? roleCodes,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var codes = SplitCodes(roleCodes);
            var result = await _permissionService.GetMyAsync(orgId, codes, false, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "permissions/me failed in DB.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/permissions/me");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load permissions." });
        }
    }

    /// <summary>GET api/permissions/modules</summary>
    [HttpGet("modules")]
    public async Task<IActionResult> ListModulesAsync(CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            return Ok(await _permissionService.ListModulesAsync(orgId, cancellationToken));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/permissions/modules");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load modules." });
        }
    }

    /// <summary>GET api/permissions/roles/{roleCode}</summary>
    [HttpGet("roles/{roleCode}")]
    public async Task<IActionResult> GetRoleAsync(
        string roleCode, CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            return Ok(await _permissionService.GetRoleAsync(orgId, roleCode, cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/permissions/roles/{{RoleCode}}");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load role permissions." });
        }
    }

    /// <summary>PUT api/permissions/roles/{roleCode}</summary>
    [HttpPut("roles/{roleCode}")]
    public async Task<IActionResult> SaveRoleAsync(
        string roleCode,
        [FromBody] SaveRolePermissionsRequest request,
        [FromQuery] string? roleCodes,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        if (!_permissionService.IsAdminRoleCodes(SplitCodes(roleCodes)))
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "Only app admins can edit permissions." });

        try
        {
            var result = await _permissionService.SaveRoleAsync(orgId, roleCode, request, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Save role permissions failed in DB.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in PUT api/permissions/roles/{{RoleCode}}");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to save permissions." });
        }
    }

    /// <summary>POST api/permissions/seed-presets</summary>
    [HttpPost("seed-presets")]
    public async Task<IActionResult> SeedPresetsAsync(
        [FromQuery] string? roleCodes,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        if (!_permissionService.IsAdminRoleCodes(SplitCodes(roleCodes)))
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "Only app admins can seed presets." });

        try
        {
            await _permissionService.SeedPresetsAsync(orgId, cancellationToken);
            return Ok(new { seeded = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in POST api/permissions/seed-presets");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to seed presets." });
        }
    }

    private static List<string> SplitCodes(string? roleCodes) =>
        string.IsNullOrWhiteSpace(roleCodes)
            ? []
            : roleCodes.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();

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
