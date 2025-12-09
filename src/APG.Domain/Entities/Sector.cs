namespace APG.Domain.Entities;

/// <summary>
/// Sector entity representing industry sectors that can be assigned to business units
/// </summary>
public class Sector : BaseEntity
{
    /// <summary>
    /// Name of the sector (e.g., Banking, Energy, Telecommunications)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if the sector is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Foreign key to the business unit this sector is assigned to (nullable)
    /// </summary>
    public int? BusinessUnitId { get; set; }

    /// <summary>
    /// Navigation property to the business unit
    /// </summary>
    public virtual BusinessUnit? BusinessUnit { get; set; }
}
