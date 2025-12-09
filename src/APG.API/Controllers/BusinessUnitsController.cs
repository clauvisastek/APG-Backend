using APG.Application.DTOs;
using APG.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APG.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BusinessUnitsController : ControllerBase
{
    private readonly IBusinessUnitService _businessUnitService;
    private readonly IBusinessUnitAccessService _businessUnitAccessService;
    private readonly ILogger<BusinessUnitsController> _logger;

    public BusinessUnitsController(
        IBusinessUnitService businessUnitService,
        IBusinessUnitAccessService businessUnitAccessService,
        ILogger<BusinessUnitsController> logger)
    {
        _businessUnitService = businessUnitService;
        _businessUnitAccessService = businessUnitAccessService;
        _logger = logger;
    }

    /// <summary>
    /// Get all business units accessible to the current user
    /// - Admin and CFO: all business units
    /// - BU leaders: only their assigned business units
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<BusinessUnitDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<BusinessUnitDto>>> GetAll()
    {
        try
        {
            // Use access service to filter BUs based on user roles
            var accessibleBusinessUnits = await _businessUnitAccessService.GetAccessibleBusinessUnitsAsync();
            
            // Map to DTOs
            var businessUnitDtos = accessibleBusinessUnits.Select(bu => new BusinessUnitDto
            {
                Id = bu.Id,
                Name = bu.Name,
                Code = bu.Code,
                ManagerName = bu.ManagerName,
                IsActive = bu.IsActive,
                CreatedAt = bu.CreatedAt,
                UpdatedAt = bu.UpdatedAt,
                Sectors = bu.Sectors?.Select(s => new SectorDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    IsActive = s.IsActive,
                    BusinessUnitId = s.BusinessUnitId
                }).ToList() ?? new List<SectorDto>()
            }).ToList();
            
            return Ok(businessUnitDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving business units");
            return StatusCode(500, new { message = "An error occurred while retrieving business units" });
        }
    }

    /// <summary>
    /// Get a business unit by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BusinessUnitDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BusinessUnitDto>> GetById(int id)
    {
        try
        {
            var businessUnit = await _businessUnitService.GetByIdAsync(id);
            
            if (businessUnit == null)
                return NotFound(new { message = $"Business unit with ID {id} not found" });

            return Ok(businessUnit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving business unit {Id}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the business unit" });
        }
    }

    /// <summary>
    /// Create a new business unit
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(BusinessUnitDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BusinessUnitDto>> Create([FromBody] BusinessUnitCreateUpdateDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var businessUnit = await _businessUnitService.CreateAsync(dto);
            
            return CreatedAtAction(
                nameof(GetById),
                new { id = businessUnit.Id },
                businessUnit);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while creating business unit");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating business unit");
            return StatusCode(500, new { message = "An error occurred while creating the business unit" });
        }
    }

    /// <summary>
    /// Update an existing business unit
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(BusinessUnitDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BusinessUnitDto>> Update(int id, [FromBody] BusinessUnitCreateUpdateDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var businessUnit = await _businessUnitService.UpdateAsync(id, dto);
            
            if (businessUnit == null)
                return NotFound(new { message = $"Business unit with ID {id} not found" });

            return Ok(businessUnit);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while updating business unit {Id}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating business unit {Id}", id);
            return StatusCode(500, new { message = "An error occurred while updating the business unit" });
        }
    }

    /// <summary>
    /// Delete a business unit
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _businessUnitService.DeleteAsync(id);
            
            if (!result)
                return NotFound(new { message = $"Business unit with ID {id} not found" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting business unit {Id}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the business unit" });
        }
    }

    /// <summary>
    /// Get Business Units accessible to the current user (based on roles)
    /// - Admin and CFO roles: can access all Business Units
    /// - BU-specific roles (e.g., "BU-001"): can only access those specific Business Units
    /// </summary>
    [HttpGet("available-for-current-user")]
    [ProducesResponseType(typeof(List<BusinessUnitDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<BusinessUnitDto>>> GetAvailableForCurrentUser()
    {
        try
        {
            var accessibleBUs = await _businessUnitAccessService.GetAccessibleBusinessUnitsAsync();
            
            var dtos = accessibleBUs.Select(bu => new BusinessUnitDto
            {
                Id = bu.Id,
                Code = bu.Code,
                Name = bu.Name,
                ManagerName = bu.ManagerName,
                IsActive = bu.IsActive,
                CreatedAt = bu.CreatedAt,
                UpdatedAt = bu.UpdatedAt,
                Sectors = new List<SectorDto>() // Empty for this endpoint
            }).ToList();

            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving accessible business units for current user");
            return StatusCode(500, new { message = "An error occurred while retrieving accessible business units" });
        }
    }
}
