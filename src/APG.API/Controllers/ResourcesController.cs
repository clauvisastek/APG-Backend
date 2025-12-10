using APG.Domain.Entities;
using APG.Persistence.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APG.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ResourcesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<ResourcesController> _logger;

    public ResourcesController(AppDbContext context, ILogger<ResourcesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all resources with optional filters
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<Resource>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<Resource>>> GetAll(
        [FromQuery] string? name,
        [FromQuery] string? email,
        [FromQuery] int? businessUnitId,
        [FromQuery] string? jobType,
        [FromQuery] string? seniority,
        [FromQuery] string? status)
    {
        try
        {
            var query = _context.Resources
                .Include(r => r.BusinessUnit)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(r => EF.Functions.Like(r.Name, $"%{name}%") || 
                                        EF.Functions.Like(r.FirstName, $"%{name}%") ||
                                        EF.Functions.Like(r.LastName, $"%{name}%"));

            if (!string.IsNullOrWhiteSpace(email))
                query = query.Where(r => r.Email.Contains(email));

            if (businessUnitId.HasValue)
                query = query.Where(r => r.BusinessUnitId == businessUnitId.Value);

            if (!string.IsNullOrWhiteSpace(jobType))
                query = query.Where(r => r.JobType == jobType);

            if (!string.IsNullOrWhiteSpace(seniority))
                query = query.Where(r => r.Seniority == seniority);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(r => r.Status == status);

            var resources = await query
                .OrderBy(r => r.Name)
                .ToListAsync();

            return Ok(resources);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving resources");
            return StatusCode(500, new { message = "An error occurred while retrieving resources" });
        }
    }

    /// <summary>
    /// Get a resource by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Resource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Resource>> GetById(int id)
    {
        try
        {
            var resource = await _context.Resources
                .Include(r => r.BusinessUnit)
                .Include(r => r.ProjectAssignments)
                    .ThenInclude(pa => pa.Project)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (resource == null)
                return NotFound(new { message = $"Resource with ID {id} not found" });

            return Ok(resource);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving resource {Id}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the resource" });
        }
    }

    /// <summary>
    /// Get a resource by email
    /// </summary>
    [HttpGet("by-email/{email}")]
    [ProducesResponseType(typeof(Resource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Resource>> GetByEmail(string email)
    {
        try
        {
            var resource = await _context.Resources
                .Include(r => r.BusinessUnit)
                .FirstOrDefaultAsync(r => r.Email.ToLower() == email.ToLower());

            if (resource == null)
                return NotFound(new { message = $"Resource with email {email} not found" });

            return Ok(resource);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving resource by email {Email}", email);
            return StatusCode(500, new { message = "An error occurred while retrieving the resource" });
        }
    }

    /// <summary>
    /// Create a new resource
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Resource), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Resource>> Create([FromBody] CreateResourceDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if email already exists
            var existingResource = await _context.Resources
                .FirstOrDefaultAsync(r => r.Email.ToLower() == dto.Email.ToLower());

            if (existingResource != null)
                return BadRequest(new { message = $"A resource with email {dto.Email} already exists", existingResource });

            // Calculate margin rate
            var marginRate = dto.DailySellRate > 0 
                ? ((dto.DailySellRate - dto.DailyCostRate) / dto.DailySellRate) * 100 
                : 0;

            var resource = new Resource
            {
                Name = $"{dto.FirstName} {dto.LastName}",
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                BusinessUnitId = dto.BusinessUnitId,
                JobType = dto.JobType,
                Seniority = dto.Seniority,
                CurrentClient = dto.CurrentClient,
                CurrentMission = dto.CurrentMission,
                Status = dto.Status ?? "Disponible",
                DailyCostRate = dto.DailyCostRate,
                DailySellRate = dto.DailySellRate,
                MarginRate = marginRate,
                HireDate = dto.HireDate,
                Manager = dto.Manager,
                Phone = dto.Phone
            };

            _context.Resources.Add(resource);
            await _context.SaveChangesAsync();

            // Reload with BusinessUnit
            await _context.Entry(resource).Reference(r => r.BusinessUnit).LoadAsync();

            return CreatedAtAction(nameof(GetById), new { id = resource.Id }, resource);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating resource");
            return StatusCode(500, new { message = "An error occurred while creating the resource" });
        }
    }

    /// <summary>
    /// Update an existing resource
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Resource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Resource>> Update(int id, [FromBody] UpdateResourceDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resource = await _context.Resources.FindAsync(id);

            if (resource == null)
                return NotFound(new { message = $"Resource with ID {id} not found" });

            // Check if email is being changed and if it's already in use
            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email.ToLower() != resource.Email.ToLower())
            {
                var existingResource = await _context.Resources
                    .FirstOrDefaultAsync(r => r.Email.ToLower() == dto.Email.ToLower());

                if (existingResource != null)
                    return BadRequest(new { message = $"A resource with email {dto.Email} already exists" });

                resource.Email = dto.Email;
            }

            if (!string.IsNullOrWhiteSpace(dto.FirstName))
                resource.FirstName = dto.FirstName;

            if (!string.IsNullOrWhiteSpace(dto.LastName))
                resource.LastName = dto.LastName;

            resource.Name = $"{resource.FirstName} {resource.LastName}";

            if (dto.BusinessUnitId.HasValue)
                resource.BusinessUnitId = dto.BusinessUnitId.Value;

            if (!string.IsNullOrWhiteSpace(dto.JobType))
                resource.JobType = dto.JobType;

            if (!string.IsNullOrWhiteSpace(dto.Seniority))
                resource.Seniority = dto.Seniority;

            if (dto.CurrentClient != null)
                resource.CurrentClient = dto.CurrentClient;

            if (dto.CurrentMission != null)
                resource.CurrentMission = dto.CurrentMission;

            if (!string.IsNullOrWhiteSpace(dto.Status))
                resource.Status = dto.Status;

            if (dto.DailyCostRate.HasValue)
                resource.DailyCostRate = dto.DailyCostRate.Value;

            if (dto.DailySellRate.HasValue)
                resource.DailySellRate = dto.DailySellRate.Value;

            // Recalculate margin rate
            resource.MarginRate = resource.DailySellRate > 0 
                ? ((resource.DailySellRate - resource.DailyCostRate) / resource.DailySellRate) * 100 
                : 0;

            if (dto.HireDate.HasValue)
                resource.HireDate = dto.HireDate.Value;

            if (dto.Manager != null)
                resource.Manager = dto.Manager;

            if (dto.Phone != null)
                resource.Phone = dto.Phone;

            await _context.SaveChangesAsync();

            // Reload with BusinessUnit
            await _context.Entry(resource).Reference(r => r.BusinessUnit).LoadAsync();

            return Ok(resource);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating resource {Id}", id);
            return StatusCode(500, new { message = "An error occurred while updating the resource" });
        }
    }

    /// <summary>
    /// Delete a resource
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var resource = await _context.Resources.FindAsync(id);

            if (resource == null)
                return NotFound(new { message = $"Resource with ID {id} not found" });

            _context.Resources.Remove(resource);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting resource {Id}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the resource" });
        }
    }

    /// <summary>
    /// Get resource mission history (all project assignments)
    /// </summary>
    [HttpGet("{id}/history")]
    [ProducesResponseType(typeof(List<ProjectResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<ProjectResource>>> GetHistory(int id)
    {
        try
        {
            var resource = await _context.Resources.FindAsync(id);

            if (resource == null)
                return NotFound(new { message = $"Resource with ID {id} not found" });

            var history = await _context.ProjectResources
                .Include(pr => pr.Project)
                    .ThenInclude(p => p.Client)
                .Where(pr => pr.ResourceId == id)
                .OrderByDescending(pr => pr.StartDate)
                .ToListAsync();

            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving history for resource {Id}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving resource history" });
        }
    }
}

// DTOs
public record CreateResourceDto(
    string FirstName,
    string LastName,
    string Email,
    int BusinessUnitId,
    string JobType,
    string Seniority,
    decimal DailyCostRate,
    decimal DailySellRate,
    DateTime HireDate,
    string? CurrentClient = null,
    string? CurrentMission = null,
    string? Status = null,
    string? Manager = null,
    string? Phone = null
);

public record UpdateResourceDto(
    string? FirstName = null,
    string? LastName = null,
    string? Email = null,
    int? BusinessUnitId = null,
    string? JobType = null,
    string? Seniority = null,
    decimal? DailyCostRate = null,
    decimal? DailySellRate = null,
    DateTime? HireDate = null,
    string? CurrentClient = null,
    string? CurrentMission = null,
    string? Status = null,
    string? Manager = null,
    string? Phone = null
);
