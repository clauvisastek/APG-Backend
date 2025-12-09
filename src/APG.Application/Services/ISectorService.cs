using APG.Application.DTOs;

namespace APG.Application.Services;

/// <summary>
/// Service interface for Sector operations
/// </summary>
public interface ISectorService
{
    /// <summary>
    /// Get all sectors available for a business unit
    /// If businessUnitId is null, returns all unassigned active sectors
    /// If businessUnitId is provided, returns unassigned sectors plus sectors already assigned to this BU
    /// </summary>
    Task<List<SectorDto>> GetAvailableForBusinessUnitAsync(int? businessUnitId);

    /// <summary>
    /// Create a new sector
    /// </summary>
    Task<SectorDto> CreateAsync(CreateSectorDto dto);

    /// <summary>
    /// Get all sectors
    /// </summary>
    Task<List<SectorDto>> GetAllAsync();
}
