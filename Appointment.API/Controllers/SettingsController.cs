using Appointment.API.Helpers;
using Appointment.Application.Services.Interfaces;
using Appointment.Domain.DTOs.OrgMasters.Requests;
using Appointment.Domain.DTOs.PublicBook.Responses;
using Appointment.Domain.DTOs.Settings.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Appointment.API.Controllers;

[Authorize]
[ApiController]
[Route("api/settings")]
public sealed class SettingsController : ControllerBase
{
    private readonly ISettingsService _settingsService;
    private readonly IOrgMastersService _orgMastersService;
    private readonly IAutoNoShowService _autoNoShowService;
    private readonly IPublicBookTokenService _tokenService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SettingsController> _logger;

    public SettingsController(
        ISettingsService settingsService,
        IOrgMastersService orgMastersService,
        IAutoNoShowService autoNoShowService,
        IPublicBookTokenService tokenService,
        IConfiguration configuration,
        ILogger<SettingsController> logger)
    {
        _settingsService = settingsService;
        _orgMastersService = orgMastersService;
        _autoNoShowService = autoNoShowService;
        _tokenService = tokenService;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>GET api/settings/masters-summary</summary>
    [HttpGet("masters-summary")]
    public async Task<IActionResult> GetMastersSummaryAsync(CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            return Ok(await _settingsService.GetMastersSummaryAsync(orgId, cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Settings masters-summary failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/settings/masters-summary");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load masters summary." });
        }
    }

    /// <summary>GET api/settings/lookups?kind=&amp;includeInactive=</summary>
    [HttpGet("lookups")]
    public async Task<IActionResult> GetLookupsAsync(
        [FromQuery] string kind,
        [FromQuery] bool includeInactive = false,
        [FromQuery] int limit = 100,
        [FromQuery] int offset = 0,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _settingsService.GetLookupsAsync(
                orgId,
                new GetLookupsRequest
                {
                    LookupKind = kind,
                    IncludeInactive = includeInactive,
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
            _logger.LogWarning(ex, "Settings lookups failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/settings/lookups");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load lookups." });
        }
    }

    /// <summary>POST api/settings/lookups</summary>
    [HttpPost("lookups")]
    public async Task<IActionResult> CreateLookupAsync(
        [FromBody] SaveLookupRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var created = await _settingsService.CreateLookupAsync(orgId, request, cancellationToken);
            return Ok(created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Create lookup failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in POST api/settings/lookups");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to create lookup." });
        }
    }

    /// <summary>PUT api/settings/lookups/{lookupId}</summary>
    [HttpPut("lookups/{lookupId:int}")]
    public async Task<IActionResult> UpdateLookupAsync(
        int lookupId,
        [FromBody] UpdateLookupRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            return Ok(await _settingsService.UpdateLookupAsync(orgId, lookupId, request, cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Update lookup failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in PUT api/settings/lookups/{LookupId}", lookupId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to update lookup." });
        }
    }

    /// <summary>DELETE api/settings/lookups/{lookupId}</summary>
    [HttpDelete("lookups/{lookupId:int}")]
    public async Task<IActionResult> DeactivateLookupAsync(
        int lookupId,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            await _settingsService.DeactivateLookupAsync(orgId, lookupId, cancellationToken);
            return Ok(new { deleted = true, lookupId });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Deactivate lookup failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in DELETE api/settings/lookups/{LookupId}", lookupId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to delete lookup." });
        }
    }

    /// <summary>GET api/settings/appointment-rules</summary>
    [HttpGet("appointment-rules")]
    public async Task<IActionResult> GetAppointmentRulesAsync(CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            return Ok(await _settingsService.GetRulesAsync(orgId, cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Get appointment rules failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/settings/appointment-rules");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load appointment rules." });
        }
    }

    /// <summary>PUT api/settings/appointment-rules</summary>
    [HttpPut("appointment-rules")]
    public async Task<IActionResult> SaveAppointmentRulesAsync(
        [FromBody] SaveAppointmentRulesRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            return Ok(await _settingsService.SetRulesAsync(orgId, request, cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Save appointment rules failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in PUT api/settings/appointment-rules");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to save appointment rules." });
        }
    }

    /// <summary>Optional manual kick for smoke tests (current org only; worker sweeps all orgs).</summary>
    [HttpPost("auto-no-show/sweep")]
    public async Task<IActionResult> SweepAutoNoShowAsync(CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            return Ok(await _autoNoShowService.SweepAsync(orgId, cancellationToken));
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Auto no-show sweep failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in POST api/settings/auto-no-show/sweep");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to run auto no-show sweep." });
        }
    }

    /// <summary>GET api/settings/branches</summary>
    [HttpGet("branches")]
    public async Task<IActionResult> ListBranchesAsync(
        [FromQuery] bool includeInactive = false,
        [FromQuery] int limit = 100,
        [FromQuery] int offset = 0,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _orgMastersService.ListBranchesAsync(
                orgId,
                new GetBranchesRequest
                {
                    IncludeInactive = includeInactive,
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
            _logger.LogWarning(ex, "Settings branches list failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/settings/branches");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load branches." });
        }
    }

    /// <summary>GET api/settings/branches/{branchId}</summary>
    [HttpGet("branches/{branchId:int}")]
    public async Task<IActionResult> GetBranchAsync(
        int branchId,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            return Ok(await _orgMastersService.GetBranchAsync(orgId, branchId, cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Settings get branch failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/settings/branches/{BranchId}", branchId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load branch." });
        }
    }

    /// <summary>POST api/settings/branches</summary>
    [HttpPost("branches")]
    public async Task<IActionResult> CreateBranchAsync(
        [FromBody] SaveBranchRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            return Ok(await _orgMastersService.CreateBranchAsync(orgId, request, cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Settings create branch failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in POST api/settings/branches");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to create branch." });
        }
    }

    /// <summary>PUT api/settings/branches/{branchId}</summary>
    [HttpPut("branches/{branchId:int}")]
    public async Task<IActionResult> UpdateBranchAsync(
        int branchId,
        [FromBody] UpdateBranchRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            return Ok(await _orgMastersService.UpdateBranchAsync(orgId, branchId, request, cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Settings update branch failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in PUT api/settings/branches/{BranchId}", branchId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to update branch." });
        }
    }

    /// <summary>DELETE api/settings/branches/{branchId} — soft-delete</summary>
    [HttpDelete("branches/{branchId:int}")]
    public async Task<IActionResult> DeactivateBranchAsync(
        int branchId,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            await _orgMastersService.DeactivateBranchAsync(orgId, branchId, cancellationToken);
            return Ok(new { branchId, deleted = true });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Settings deactivate branch failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in DELETE api/settings/branches/{BranchId}", branchId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to delete branch." });
        }
    }

    /// <summary>GET api/settings/public-booking-link — encrypted QR token for this org (no raw org id in customer URL).</summary>
    [HttpGet("public-booking-link")]
    public IActionResult GetPublicBookingLink()
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var token = _tokenService.CreateToken(orgId);
            var path = $"/public/book?t={Uri.EscapeDataString(token)}";
            var frontendBase = (_configuration["Appointment:FrontendBaseUrl"] ?? "https://appointment.softoncloud.com")
                .TrimEnd('/');
            return Ok(new PublicBookingLinkDto
            {
                Token = token,
                BookingPath = path,
                BookingUrl = $"{frontendBase}{path}",
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create public booking link for org {OrgId}", orgId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to create booking link." });
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
