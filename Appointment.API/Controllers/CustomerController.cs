using Appointment.API.Helpers;
using Appointment.Application.Services.Interfaces;
using Appointment.Domain.DTOs.Customers.Requests;
using Appointment.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace Appointment.API.Controllers;

[Authorize]
[ApiController]
[Route("api/customers")]
public sealed class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomerController> _logger;

    public CustomerController(ICustomerService customerService, ILogger<CustomerController> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    /// <summary>GET api/customers?search=&amp;status=&amp;accountTypeId=&amp;limit=&amp;offset=</summary>
    [HttpGet]
    public async Task<IActionResult> GetCustomersAsync(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] int? accountTypeId,
        [FromQuery] int limit = 50,
        [FromQuery] int offset = 0,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out var userId, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        _ = userId;

        try
        {
            var result = await _customerService.GetCustomersAsync(
                orgId,
                new GetCustomersRequest
                {
                    Search = search,
                    Status = status,
                    AccountTypeId = accountTypeId,
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
            _logger.LogWarning(ex, "Customer list failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/customers");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load customers." });
        }
    }

    /// <summary>GET api/customers/account-types</summary>
    [HttpGet("account-types")]
    public async Task<IActionResult> GetAccountTypesAsync(CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _customerService.GetAccountTypesAsync(orgId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Account types failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/customers/account-types");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load account types." });
        }
    }

    /// <summary>GET api/customers/stats</summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetCustomerStatsAsync(CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _customerService.GetCustomerStatsAsync(orgId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Customer stats failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/customers/stats");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load customer stats." });
        }
    }

    /// <summary>GET api/customers/{accountId}</summary>
    [HttpGet("{accountId:int}")]
    public async Task<IActionResult> GetCustomerByIdAsync(
        int accountId,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _customerService.GetCustomerByIdAsync(orgId, accountId, cancellationToken);
            if (result is null)
                return NotFound(new { message = "Customer not found." });

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Get customer failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET api/customers/{AccountId}", accountId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to load customer." });
        }
    }

    /// <summary>POST api/customers</summary>
    [HttpPost]
    public async Task<IActionResult> CreateCustomerAsync(
        [FromBody] CreateCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out var userId, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        if (request is null)
            return BadRequest(new { message = "Request body is required." });

        try
        {
            var result = await _customerService.CreateCustomerAsync(orgId, userId, request, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (CustomerDuplicateEmailException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Create customer failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in POST api/customers");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to create customer." });
        }
    }

    /// <summary>PUT api/customers/{accountId}</summary>
    [HttpPut("{accountId:int}")]
    public async Task<IActionResult> UpdateCustomerAsync(
        int accountId,
        [FromBody] UpdateCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        if (request is null)
            return BadRequest(new { message = "Request body is required." });

        try
        {
            var result = await _customerService.UpdateCustomerAsync(orgId, accountId, request, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (CustomerDuplicateEmailException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Update customer failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in PUT api/customers/{AccountId}", accountId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to update customer." });
        }
    }

    /// <summary>DELETE api/customers/{accountId} — soft delete</summary>
    [HttpDelete("{accountId:int}")]
    public async Task<IActionResult> DeactivateCustomerAsync(
        int accountId,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthContext(out _, out var orgId))
            return Unauthorized(new { message = "Invalid or missing authentication token." });

        try
        {
            var result = await _customerService.DeactivateCustomerAsync(orgId, accountId, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "Deactivate customer failed in DB function.");
            return BadRequest(new { message = AuthExceptionHelper.GetUserFacingMessage(ex) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in DELETE api/customers/{AccountId}", accountId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to delete customer." });
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
