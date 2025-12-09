namespace APG.Domain.Entities;

/// <summary>
/// Business Unit entity representing organizational units within the company
/// </summary>
public class BusinessUnit : BaseEntity
{
    /// <summary>
    /// Name of the business unit
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Unique code for the business unit (e.g., BU-001, BU-002)
    /// Generated automatically by the backend
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Name of the manager/responsible person for this business unit
    /// </summary>
    public string ManagerName { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if the business unit is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Collection of sectors associated with this business unit
    /// </summary>
    public virtual ICollection<Sector> Sectors { get; set; } = new List<Sector>();
}
