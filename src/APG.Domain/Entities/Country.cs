namespace APG.Domain.Entities;

/// <summary>
/// Country entity for location reference data
/// </summary>
public class Country : BaseEntity
{
    /// <summary>
    /// Country name (e.g., "Canada", "France")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// ISO country code (e.g., "CA", "FR") - optional
    /// </summary>
    public string? IsoCode { get; set; }

    /// <summary>
    /// Indicates if this country is active and available for selection
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<Client> Clients { get; set; } = new List<Client>();
}
