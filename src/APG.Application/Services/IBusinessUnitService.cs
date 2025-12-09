using APG.Application.DTOs;

namespace APG.Application.Services;

/// <summary>
/// Service interface for Business Unit operations
/// </summary>
public interface IBusinessUnitService
{
    /// <summary>
    /// Get all business units
    /// </summary>
    Task<List<BusinessUnitDto>> GetAllAsync();

    /// <summary>
    /// Get a business unit by ID
    /// </summary>
    Task<BusinessUnitDto?> GetByIdAsync(int id);

    /// <summary>
    /// Create a new business unit
    /// </summary>
    Task<BusinessUnitDto> CreateAsync(BusinessUnitCreateUpdateDto dto);

    /// <summary>
    /// Update an existing business unit
    /// </summary>
    Task<BusinessUnitDto?> UpdateAsync(int id, BusinessUnitCreateUpdateDto dto);

    /// <summary>
    /// Delete a business unit
    /// </summary>
    Task<bool> DeleteAsync(int id);
}
