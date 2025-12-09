using APG.Application.DTOs;

namespace APG.Application.Services;

/// <summary>
/// Service for calculator settings operations (CFO configuration)
/// </summary>
public interface ICalculatorSettingsService
{
    /// <summary>
    /// Get current global salary settings
    /// </summary>
    Task<GlobalSalarySettingsDto?> GetCurrentGlobalSettingsAsync();

    /// <summary>
    /// Create or update global salary settings
    /// </summary>
    Task<GlobalSalarySettingsDto> UpsertGlobalSettingsAsync(UpdateGlobalSalarySettingsRequest request);

    /// <summary>
    /// Get all client margin settings
    /// </summary>
    Task<List<ClientMarginSettingsDto>> GetClientMarginSettingsAsync();

    /// <summary>
    /// Create new client margin settings
    /// </summary>
    Task<ClientMarginSettingsDto> CreateClientMarginSettingsAsync(CreateClientMarginSettingsRequest request);

    /// <summary>
    /// Update existing client margin settings
    /// </summary>
    Task<ClientMarginSettingsDto> UpdateClientMarginSettingsAsync(int id, UpdateClientMarginSettingsRequest request);

    /// <summary>
    /// Delete client margin settings
    /// </summary>
    Task DeleteClientMarginSettingsAsync(int id);
}
