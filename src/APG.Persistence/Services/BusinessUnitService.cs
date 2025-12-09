using APG.Application.DTOs;
using APG.Application.Services;
using APG.Domain.Entities;
using APG.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace APG.Persistence.Services;

/// <summary>
/// Service implementation for Business Unit operations
/// </summary>
public class BusinessUnitService : IBusinessUnitService
{
    private readonly AppDbContext _context;

    public BusinessUnitService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<BusinessUnitDto>> GetAllAsync()
    {
        var businessUnits = await _context.BusinessUnits
            .Include(bu => bu.Sectors)
            .Where(bu => bu.IsActive)
            .OrderBy(bu => bu.Code)
            .ToListAsync();

        return businessUnits.Select(MapToDto).ToList();
    }

    public async Task<BusinessUnitDto?> GetByIdAsync(int id)
    {
        var businessUnit = await _context.BusinessUnits
            .Include(bu => bu.Sectors)
            .FirstOrDefaultAsync(bu => bu.Id == id);

        return businessUnit == null ? null : MapToDto(businessUnit);
    }

    public async Task<BusinessUnitDto> CreateAsync(BusinessUnitCreateUpdateDto dto)
    {
        // Generate the next code
        var nextCode = await GenerateNextCodeAsync();

        // Validate sectors
        await ValidateSectorsAsync(dto.SectorIds, null);

        // Create business unit
        var businessUnit = new BusinessUnit
        {
            Name = dto.Name.Trim(),
            Code = nextCode,
            ManagerName = dto.ManagerName.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.BusinessUnits.Add(businessUnit);
        await _context.SaveChangesAsync();

        // Attach sectors
        if (dto.SectorIds.Any())
        {
            await AttachSectorsAsync(businessUnit.Id, dto.SectorIds);
        }

        // Reload with sectors
        await _context.Entry(businessUnit).Collection(bu => bu.Sectors).LoadAsync();

        return MapToDto(businessUnit);
    }

    public async Task<BusinessUnitDto?> UpdateAsync(int id, BusinessUnitCreateUpdateDto dto)
    {
        var businessUnit = await _context.BusinessUnits
            .Include(bu => bu.Sectors)
            .FirstOrDefaultAsync(bu => bu.Id == id);

        if (businessUnit == null)
            return null;

        // Validate sectors
        await ValidateSectorsAsync(dto.SectorIds, id);

        // Update properties
        businessUnit.Name = dto.Name.Trim();
        businessUnit.ManagerName = dto.ManagerName.Trim();
        businessUnit.UpdatedAt = DateTime.UtcNow;

        // Update sectors - detach current sectors first
        var currentSectors = businessUnit.Sectors.ToList();
        foreach (var sector in currentSectors)
        {
            sector.BusinessUnitId = null;
        }

        // Attach new sectors
        if (dto.SectorIds.Any())
        {
            var sectorsToAttach = await _context.Sectors
                .Where(s => dto.SectorIds.Contains(s.Id))
                .ToListAsync();

            foreach (var sector in sectorsToAttach)
            {
                sector.BusinessUnitId = id;
            }
        }

        await _context.SaveChangesAsync();

        // Reload with updated sectors
        await _context.Entry(businessUnit).Collection(bu => bu.Sectors).LoadAsync();

        return MapToDto(businessUnit);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var businessUnit = await _context.BusinessUnits
            .Include(bu => bu.Sectors)
            .FirstOrDefaultAsync(bu => bu.Id == id);

        if (businessUnit == null)
            return false;

        // Detach all sectors before deleting
        foreach (var sector in businessUnit.Sectors)
        {
            sector.BusinessUnitId = null;
        }

        _context.BusinessUnits.Remove(businessUnit);
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Generate the next business unit code in the format BU-XXX
    /// </summary>
    private async Task<string> GenerateNextCodeAsync()
    {
        var lastCode = await _context.BusinessUnits
            .OrderByDescending(bu => bu.Code)
            .Select(bu => bu.Code)
            .FirstOrDefaultAsync();

        if (string.IsNullOrEmpty(lastCode))
        {
            return "BU-001";
        }

        // Extract numeric part from last code (e.g., "BU-005" -> 5)
        var match = Regex.Match(lastCode, @"BU-(\d+)");
        if (match.Success && int.TryParse(match.Groups[1].Value, out var lastNumber))
        {
            var nextNumber = lastNumber + 1;
            return $"BU-{nextNumber:D3}"; // Format with 3 digits, zero-padded
        }

        // Fallback if parsing fails
        return "BU-001";
    }

    /// <summary>
    /// Validate that sectors can be attached to the business unit
    /// </summary>
    private async Task ValidateSectorsAsync(List<int> sectorIds, int? businessUnitId)
    {
        if (!sectorIds.Any())
            return;

        var sectors = await _context.Sectors
            .Where(s => sectorIds.Contains(s.Id))
            .ToListAsync();

        // Check if all requested sectors exist
        if (sectors.Count != sectorIds.Count)
        {
            var missingSectorIds = sectorIds.Except(sectors.Select(s => s.Id)).ToList();
            throw new InvalidOperationException(
                $"Sectors not found: {string.Join(", ", missingSectorIds)}");
        }

        // Check if any sector is already attached to another business unit
        var conflictingSectors = sectors
            .Where(s => s.BusinessUnitId.HasValue && s.BusinessUnitId != businessUnitId)
            .ToList();

        if (conflictingSectors.Any())
        {
            var conflictingNames = string.Join(", ", conflictingSectors.Select(s => $"'{s.Name}'"));
            throw new InvalidOperationException(
                $"The following sectors are already assigned to other business units: {conflictingNames}");
        }
    }

    /// <summary>
    /// Attach sectors to a business unit
    /// </summary>
    private async Task AttachSectorsAsync(int businessUnitId, List<int> sectorIds)
    {
        var sectors = await _context.Sectors
            .Where(s => sectorIds.Contains(s.Id))
            .ToListAsync();

        foreach (var sector in sectors)
        {
            sector.BusinessUnitId = businessUnitId;
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Map BusinessUnit entity to DTO
    /// </summary>
    private static BusinessUnitDto MapToDto(BusinessUnit businessUnit)
    {
        return new BusinessUnitDto
        {
            Id = businessUnit.Id,
            Name = businessUnit.Name,
            Code = businessUnit.Code,
            ManagerName = businessUnit.ManagerName,
            IsActive = businessUnit.IsActive,
            CreatedAt = businessUnit.CreatedAt,
            UpdatedAt = businessUnit.UpdatedAt,
            Sectors = businessUnit.Sectors.Select(s => new SectorDto
            {
                Id = s.Id,
                Name = s.Name,
                IsActive = s.IsActive,
                BusinessUnitId = s.BusinessUnitId
            }).ToList()
        };
    }
}
