using Appointment.API.Helpers;
using Appointment.Application.Services.Interfaces;
using Appointment.Domain.DTOs.Auth.Requests;
using Appointment.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace Appointment.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger      = logger;
    }

    /// <summary>
    /// POST api/auth/login
    /// Body: { orgId, email, password, rememberMe }
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
            return BadRequest(new { message = "Request body is required." });

        try
        {
            var ipAddress  = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent  = Request.Headers.UserAgent.ToString();
            var deviceInfo = Request.Headers["Sec-CH-UA-Platform"].ToString();

            var response = await _authService.LoginAsync(
                request,
                ipAddress,
                userAgent,
                string.IsNullOrWhiteSpace(deviceInfo) ? null : deviceInfo,
                cancellationToken);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (LoginUnauthorizedException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Login failed in DB function for orgId {OrgId}", request.OrgId);
            return Unauthorized(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Login failed due to configuration issue.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in POST api/auth/login");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred during login." });
        }
    }

    /// <summary>
    /// GET api/auth/resolve-context?productId={id}
    /// Requires Bearer token. userId and orgId are read from the JWT.
    /// </summary>
    [Authorize]
    [HttpGet("resolve-context")]
    public async Task<IActionResult> ResolveUserContextAsync(
        [FromQuery] int productId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!TryGetAuthContext(out var userId, out var orgId, out _))
            {
                return Unauthorized(new { message = "Invalid or missing authentication token." });
            }

            if (productId <= 0)
            {
                return BadRequest(new { message = "Product ID is required." });
            }

            var response = await _authService.ResolveUserContextAsync(
                userId,
                orgId,
                productId,
                cancellationToken);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ProductAccessDeniedException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                message = ex.Message,
                denialReason = ex.DenialReason
            });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "User context resolution failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "User context resolution failed due to invalid response.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/auth/resolve-context");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to resolve user context." });
        }
    }

    private bool TryGetAuthContext(out int userId, out int orgId, out string? identityType)
    {
        userId = 0;
        orgId = 0;
        identityType = User.FindFirst("identity_type")?.Value;

        var userIdRaw = User.FindFirst("user_id")?.Value;
        var orgIdRaw = User.FindFirst("org_id")?.Value;

        return int.TryParse(userIdRaw, out userId)
               && userId > 0
               && int.TryParse(orgIdRaw, out orgId)
               && orgId > 0;
    }
}
