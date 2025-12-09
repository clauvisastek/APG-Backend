using APG.Application.DTOs;
using APG.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APG.API.Controllers;

/// <summary>
/// Controller for calculator settings (CFO configuration)
/// </summary>
[ApiController]
[Route("api/calculator-settings")]
[Authorize(Roles = "Admin,CFO")]
public class CalculatorSettingsController : ControllerBase
{
    private readonly ICalculatorSettingsService _calculatorSettingsService;
    private readonly ILogger<CalculatorSettingsController> _logger;

    public CalculatorSettingsController(
        ICalculatorSettingsService calculatorSettingsService,
        ILogger<CalculatorSettingsController> logger)
    {
        _calculatorSettingsService = calculatorSettingsService;
        _logger = logger;
    }

    /// <summary>
    /// Get current global salary settings
    /// </summary>
    [HttpGet("global-salary")]
    [ProducesResponseType(typeof(GlobalSalarySettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<GlobalSalarySettingsDto?>> GetGlobalSalarySettings()
    {
        try
        {
            var settings = await _calculatorSettingsService.GetCurrentGlobalSettingsAsync();
            
            if (settings == null)
                return NoContent();
            
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving global salary settings");
            return StatusCode(500, new { message = "An error occurred while retrieving global salary settings" });
        }
    }

    /// <summary>
    /// Create or update global salary settings
    /// </summary>
    [HttpPut("global-salary")]
    [ProducesResponseType(typeof(GlobalSalarySettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GlobalSalarySettingsDto>> UpsertGlobalSalarySettings(
        [FromBody] UpdateGlobalSalarySettingsRequest request)
    {
        try
        {
            var settings = await _calculatorSettingsService.UpsertGlobalSettingsAsync(request);
            return Ok(settings);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error for global salary settings");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upserting global salary settings");
            return StatusCode(500, new { message = "An error occurred while saving global salary settings" });
        }
    }

    /// <summary>
    /// Get all client margin settings
    /// </summary>
    [HttpGet("client-margins")]
    [ProducesResponseType(typeof(List<ClientMarginSettingsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ClientMarginSettingsDto>>> GetClientMarginSettings()
    {
        try
        {
            var settings = await _calculatorSettingsService.GetClientMarginSettingsAsync();
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving client margin settings");
            return StatusCode(500, new { message = "An error occurred while retrieving client margin settings" });
        }
    }

    /// <summary>
    /// Create new client margin settings
    /// </summary>
    [HttpPost("client-margins")]
    [ProducesResponseType(typeof(ClientMarginSettingsDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientMarginSettingsDto>> CreateClientMarginSettings(
        [FromBody] CreateClientMarginSettingsRequest request)
    {
        try
        {
            var settings = await _calculatorSettingsService.CreateClientMarginSettingsAsync(request);
            return CreatedAtAction(
                nameof(GetClientMarginSettings),
                new { id = settings.Id },
                settings);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Client not found: {ClientId}", request.ClientId);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Client margin settings already exist for client: {ClientId}", request.ClientId);
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error for client margin settings");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating client margin settings");
            return StatusCode(500, new { message = "An error occurred while creating client margin settings" });
        }
    }

    /// <summary>
    /// Update existing client margin settings
    /// </summary>
    [HttpPut("client-margins/{id}")]
    [ProducesResponseType(typeof(ClientMarginSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientMarginSettingsDto>> UpdateClientMarginSettings(
        int id,
        [FromBody] UpdateClientMarginSettingsRequest request)
    {
        try
        {
            var settings = await _calculatorSettingsService.UpdateClientMarginSettingsAsync(id, request);
            return Ok(settings);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Client margin settings not found: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error for client margin settings");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating client margin settings");
            return StatusCode(500, new { message = "An error occurred while updating client margin settings" });
        }
    }

    /// <summary>
    /// Delete client margin settings
    /// </summary>
    [HttpDelete("client-margins/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteClientMarginSettings(int id)
    {
        try
        {
            await _calculatorSettingsService.DeleteClientMarginSettingsAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Client margin settings not found: {Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting client margin settings");
            return StatusCode(500, new { message = "An error occurred while deleting client margin settings" });
        }
    }
}
