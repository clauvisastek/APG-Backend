namespace APG.Domain.Entities;

/// <summary>
/// Currency entity for financial reference data
/// </summary>
public class Currency : BaseEntity
{
    /// <summary>
    /// Currency name (e.g., "CAD (Canadian Dollar)")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Currency code (e.g., "CAD", "USD", "EUR")
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Currency symbol (e.g., "$", "€") - optional
    /// </summary>
    public string? Symbol { get; set; }

    /// <summary>
    /// Indicates if this currency is active and available for selection
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<Client> Clients { get; set; } = new List<Client>();
}
