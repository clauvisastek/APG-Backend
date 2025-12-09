using APG.Application.DTOs;
using APG.Application.Services;
using APG.Domain.Entities;
using APG.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace APG.Persistence.Services;

/// <summary>
/// Service implementation for Sector operations
/// </summary>
public class SectorService : ISectorService
{
    private readonly AppDbContext _context;

    public SectorService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<SectorDto>> GetAvailableForBusinessUnitAsync(int? businessUnitId)
    {
        IQueryable<Sector> query = _context.Sectors.Where(s => s.IsActive);

        if (businessUnitId.HasValue)
        {
            // Return sectors that are either unassigned OR already assigned to this specific BU
            query = query.Where(s => s.BusinessUnitId == null || s.BusinessUnitId == businessUnitId.Value);
        }
        else
        {
            // Return only unassigned sectors
            query = query.Where(s => s.BusinessUnitId == null);
        }

        var sectors = await query.OrderBy(s => s.Name).ToListAsync();

        return sectors.Select(MapToDto).ToList();
    }

    public async Task<SectorDto> CreateAsync(CreateSectorDto dto)
    {
        var name = NormalizeName(dto.Name);

        // Check if a sector with this name already exists
        var existingSector = await _context.Sectors
            .FirstOrDefaultAsync(s => s.Name.ToLower() == name.ToLower());

        if (existingSector != null)
        {
            throw new InvalidOperationException($"A sector with the name '{name}' already exists.");
        }

        var sector = new Sector
        {
            Name = name,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Sectors.Add(sector);
        await _context.SaveChangesAsync();

        return MapToDto(sector);
    }

    public async Task<List<SectorDto>> GetAllAsync()
    {
        var sectors = await _context.Sectors
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync();

        return sectors.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Normalize sector name: trim and convert to title case
    /// </summary>
    private static string NormalizeName(string name)
    {
        name = name.Trim();
        
        if (string.IsNullOrEmpty(name))
            return name;

        // Convert to title case (first letter uppercase, rest lowercase)
        return char.ToUpper(name[0]) + name.Substring(1).ToLower();
    }

    /// <summary>
    /// Map Sector entity to DTO
    /// </summary>
    private static SectorDto MapToDto(Sector sector)
    {
        return new SectorDto
        {
            Id = sector.Id,
            Name = sector.Name,
            IsActive = sector.IsActive,
            BusinessUnitId = sector.BusinessUnitId
        };
    }
}
