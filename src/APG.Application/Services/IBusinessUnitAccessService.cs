using APG.Domain.Entities;

namespace APG.Application.Services;

/// <summary>
/// Service for determining which Business Units a user can access based on their roles
/// </summary>
public interface IBusinessUnitAccessService
{
    /// <summary>
    /// Get the list of Business Units accessible to the current user based on their roles
    /// - Admin and CFO roles: can access all Business Units
    /// - BU-specific roles (e.g., "BU-001"): can only access those specific Business Units
    /// </summary>
    Task<List<BusinessUnit>> GetAccessibleBusinessUnitsAsync();

    /// <summary>
    /// Check if the current user can access a specific Business Unit
    /// </summary>
    Task<bool> CanAccessBusinessUnitAsync(int businessUnitId);
}
