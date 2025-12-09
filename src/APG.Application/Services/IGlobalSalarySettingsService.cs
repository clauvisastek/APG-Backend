using APG.Application.DTOs;

namespace APG.Application.Services;

/// <summary>
/// Service for managing global salary settings with activation workflow
/// </summary>
public interface IGlobalSalarySettingsService
{
    /// <summary>
    /// Get the currently active global salary settings
    /// </summary>
    Task<ActiveGlobalSalarySettingsResponse> GetActiveAsync();

    /// <summary>
    /// Get all global salary settings (full history)
    /// </summary>
    Task<List<GlobalSalarySettingsDto>> GetAllAsync();

    /// <summary>
    /// Create new global salary settings and automatically activate it
    /// </summary>
    Task<GlobalSalarySettingsDto> CreateAsync(CreateGlobalSalarySettingsRequest request);

    /// <summary>
    /// Update existing global salary settings (keeps activation state)
    /// </summary>
    Task<GlobalSalarySettingsDto> UpdateAsync(int id, UpdateGlobalSalarySettingsRequest request);

    /// <summary>
    /// Activate a specific global salary settings record
    /// </summary>
    Task<GlobalSalarySettingsDto> ActivateAsync(int id);

    /// <summary>
    /// Delete a global salary settings record (only if not active)
    /// </summary>
    Task DeleteAsync(int id);
}
