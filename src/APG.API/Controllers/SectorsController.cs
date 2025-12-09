using APG.Application.DTOs;
using APG.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace APG.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SectorsController : ControllerBase
{
    private readonly ISectorService _sectorService;
    private readonly ILogger<SectorsController> _logger;

    public SectorsController(
        ISectorService sectorService,
        ILogger<SectorsController> logger)
    {
        _sectorService = sectorService;
        _logger = logger;
    }

    /// <summary>
    /// Get all sectors
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<SectorDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<SectorDto>>> GetAll()
    {
        try
        {
            var sectors = await _sectorService.GetAllAsync();
            return Ok(sectors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sectors");
            return StatusCode(500, new { message = "An error occurred while retrieving sectors" });
        }
    }

    /// <summary>
    /// Get sectors available for assignment to a business unit
    /// If businessUnitId is not provided, returns all unassigned sectors
    /// If businessUnitId is provided, returns unassigned sectors plus sectors already assigned to that BU
    /// </summary>
    [HttpGet("available-for-business-unit/{businessUnitId?}")]
    [ProducesResponseType(typeof(List<SectorDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<SectorDto>>> GetAvailableForBusinessUnit(int? businessUnitId = null)
    {
        try
        {
            var sectors = await _sectorService.GetAvailableForBusinessUnitAsync(businessUnitId);
            return Ok(sectors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available sectors for business unit {BusinessUnitId}", businessUnitId);
            return StatusCode(500, new { message = "An error occurred while retrieving available sectors" });
        }
    }

    /// <summary>
    /// Create a new sector
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SectorDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SectorDto>> Create([FromBody] CreateSectorDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { message = "Sector name is required" });

            var sector = await _sectorService.CreateAsync(dto);
            
            return CreatedAtAction(
                nameof(GetAll),
                new { id = sector.Id },
                sector);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while creating sector");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating sector");
            return StatusCode(500, new { message = "An error occurred while creating the sector" });
        }
    }
}
