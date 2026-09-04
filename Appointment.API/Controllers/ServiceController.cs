using Appointment.API.Helpers;
using Appointment.Application.Services.Interfaces;
using Appointment.Domain.DTOs.Services.Requests;
using Appointment.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace Appointment.API.Controllers;

[Authorize]
[ApiController]
[Route("api/services")]
public sealed class ServiceController : ControllerBase
{
    private readonly IServiceCatalogService _serviceCatalogService;
    private readonly ILogger<ServiceController> _logger;

    public ServiceController(IServiceCatalogService serviceCatalogService, ILogger<ServiceController> logger)
    {
        _serviceCatalogService = serviceCatalogService;
        _logger = logger;
    }

    /// <summary>GET api/services?search=&amp;isActive=&amp;limit=&amp;offset=</summary>
    [HttpGet]
    public async Task<IActionResult> GetServicesAsync(
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        [FromQuery] int limit = 50,
        [FromQuery] int offset = 0,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _serviceCatalogService.GetServicesAsync(
                orgId,
                new GetServicesRequest
                {
                    Search = search,
                    IsActive = isActive,
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
            _logger.LogWarning(ex, "Service list failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/services");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load services." });
        }
    }

    /// <summary>GET api/services/categories</summary>
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _serviceCatalogService.GetCategoriesAsync(orgId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Service categories failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/services/categories");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load categories." });
        }
    }

    /// <summary>GET api/services/{productId}</summary>
    [HttpGet("{productId:int}")]
    public async Task<IActionResult> GetServiceByIdAsync(
        int productId,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _serviceCatalogService.GetServiceByIdAsync(orgId, productId, cancellationToken);
            if (result is null)
                return NotFound(new { message = "Service not found." });

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Get service failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/services/{ProductId}", productId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load service." });
        }
    }

    /// <summary>POST api/services</summary>
    [HttpPost]
    public async Task<IActionResult> CreateServiceAsync(
        [FromBody] CreateServiceRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        if (request is null)
            return BadRequest(new { message = "Request body is required." });

        try
        {
            var result = await _serviceCatalogService.CreateServiceAsync(orgId, request, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ServiceDuplicateNameException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Create service failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in POST api/services");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to create service." });
        }
    }

    /// <summary>PUT api/services/{productId}</summary>
    [HttpPut("{productId:int}")]
    public async Task<IActionResult> UpdateServiceAsync(
        int productId,
        [FromBody] UpdateServiceRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        if (request is null)
            return BadRequest(new { message = "Request body is required." });

        try
        {
            var result = await _serviceCatalogService.UpdateServiceAsync(orgId, productId, request, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ServiceDuplicateNameException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Update service failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in PUT api/services/{ProductId}", productId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to update service." });
        }
    }

    /// <summary>DELETE api/services/{productId} — sets is_active false</summary>
    [HttpDelete("{productId:int}")]
    public async Task<IActionResult> DeactivateServiceAsync(
        int productId,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _serviceCatalogService.DeactivateServiceAsync(orgId, productId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Deactivate service failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in DELETE api/services/{ProductId}", productId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to deactivate service." });
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
