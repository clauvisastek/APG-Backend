using APG.Application.DTOs;
using APG.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APG.API.Controllers;

/// <summary>
/// Controller for managing global salary settings (CFO configuration)
/// </summary>
[ApiController]
[Route("api/salary-settings")]
[Authorize(Roles = "Admin,CFO")]
public class GlobalSalarySettingsController : ControllerBase
{
    private readonly IGlobalSalarySettingsService _service;
    private readonly ILogger<GlobalSalarySettingsController> _logger;

    public GlobalSalarySettingsController(
        IGlobalSalarySettingsService service,
        ILogger<GlobalSalarySettingsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Get the currently active global salary settings
    /// </summary>
    /// <returns>Active settings or indication that none are configured</returns>
    /// <response code="200">Returns the active settings or HasActiveSettings=false</response>
    [HttpGet("active")]
    [ProducesResponseType(typeof(ActiveGlobalSalarySettingsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ActiveGlobalSalarySettingsResponse>> GetActive()
    {
        try
        {
            var response = await _service.GetActiveAsync();
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active global salary settings");
            return Problem(
                title: "Error retrieving active settings",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Get all global salary settings (full history)
    /// </summary>
    /// <returns>List of all global salary settings ordered by creation date</returns>
    /// <response code="200">Returns the list of settings</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<GlobalSalarySettingsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<GlobalSalarySettingsDto>>> GetAll()
    {
        try
        {
            var settings = await _service.GetAllAsync();
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all global salary settings");
            return Problem(
                title: "Error retrieving settings",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Create new global salary settings
    /// </summary>
    /// <param name="request">Settings to create</param>
    /// <returns>The created settings (automatically activated)</returns>
    /// <response code="201">Settings created successfully</response>
    /// <response code="400">Invalid input</response>
    [HttpPost]
    [ProducesResponseType(typeof(GlobalSalarySettingsDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GlobalSalarySettingsDto>> Create([FromBody] CreateGlobalSalarySettingsRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var created = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetAll), new { }, created);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error creating global salary settings");
            return Problem(
                title: "Validation error",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating global salary settings");
            return Problem(
                title: "Error creating settings",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Update existing global salary settings
    /// </summary>
    /// <param name="id">Settings ID</param>
    /// <param name="request">Updated values</param>
    /// <returns>The updated settings</returns>
    /// <response code="200">Settings updated successfully</response>
    /// <response code="400">Invalid input</response>
    /// <response code="404">Settings not found</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(GlobalSalarySettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GlobalSalarySettingsDto>> Update(int id, [FromBody] UpdateGlobalSalarySettingsRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updated = await _service.UpdateAsync(id, request);
            return Ok(updated);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Global salary settings with ID {Id} not found", id);
            return NotFound(new ProblemDetails
            {
                Title = "Settings not found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error updating global salary settings");
            return Problem(
                title: "Validation error",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating global salary settings with ID {Id}", id);
            return Problem(
                title: "Error updating settings",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Activate a specific global salary settings record
    /// </summary>
    /// <param name="id">Settings ID to activate</param>
    /// <returns>The activated settings</returns>
    /// <response code="200">Settings activated successfully</response>
    /// <response code="404">Settings not found</response>
    [HttpPost("{id}/activate")]
    [ProducesResponseType(typeof(GlobalSalarySettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GlobalSalarySettingsDto>> Activate(int id)
    {
        try
        {
            var activated = await _service.ActivateAsync(id);
            return Ok(activated);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Global salary settings with ID {Id} not found", id);
            return NotFound(new ProblemDetails
            {
                Title = "Settings not found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating global salary settings with ID {Id}", id);
            return Problem(
                title: "Error activating settings",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Delete a global salary settings record
    /// </summary>
    /// <param name="id">Settings ID to delete</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Settings deleted successfully</response>
    /// <response code="400">Cannot delete active settings or last remaining settings</response>
    /// <response code="404">Settings not found</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Global salary settings with ID {Id} not found", id);
            return NotFound(new ProblemDetails
            {
                Title = "Settings not found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot delete global salary settings with ID {Id}", id);
            return Problem(
                title: "Cannot delete settings",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting global salary settings with ID {Id}", id);
            return Problem(
                title: "Error deleting settings",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}
