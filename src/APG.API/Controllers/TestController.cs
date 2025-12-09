using APG.Domain.Entities;
using APG.Persistence.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APG.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<TestController> _logger;

    public TestController(AppDbContext context, ILogger<TestController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all test entities
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TestEntity>>> GetAll()
    {
        try
        {
            var entities = await _context.TestEntities.ToListAsync();
            return Ok(entities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving test entities");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get a test entity by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<TestEntity>> GetById(int id)
    {
        var entity = await _context.TestEntities.FindAsync(id);
        
        if (entity == null)
        {
            return NotFound();
        }
        
        return Ok(entity);
    }

    /// <summary>
    /// Create a new test entity
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TestEntity>> Create(CreateTestEntityRequest request)
    {
        var entity = new TestEntity
        {
            Name = request.Name,
            Description = request.Description,
            IsActive = request.IsActive
        };

        _context.TestEntities.Add(entity);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    /// <summary>
    /// Update an existing test entity
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateTestEntityRequest request)
    {
        var entity = await _context.TestEntities.FindAsync(id);
        
        if (entity == null)
        {
            return NotFound();
        }

        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.IsActive = request.IsActive;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Delete a test entity
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.TestEntities.FindAsync(id);
        
        if (entity == null)
        {
            return NotFound();
        }

        _context.TestEntities.Remove(entity);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public record CreateTestEntityRequest(string Name, string? Description, bool IsActive = true);
public record UpdateTestEntityRequest(string Name, string? Description, bool IsActive);
