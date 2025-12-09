namespace APG.Domain.Entities;

/// <summary>
/// Client entity representing customers/clients in the system
/// </summary>
public class Client : BaseEntity
{
    /// <summary>
    /// Client code - entered by user (e.g., "ACME", "TECHSTART")
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Client name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Foreign key to Sector
    /// </summary>
    public int SectorId { get; set; }

    /// <summary>
    /// Foreign key to Business Unit
    /// </summary>
    public int BusinessUnitId { get; set; }

    /// <summary>
    /// Foreign key to Country
    /// </summary>
    public int CountryId { get; set; }

    /// <summary>
    /// Foreign key to Currency
    /// </summary>
    public int CurrencyId { get; set; }

    /// <summary>
    /// Default target margin percentage for this client (optional, only for Admin/CFO)
    /// </summary>
    public decimal? DefaultTargetMarginPercent { get; set; }

    /// <summary>
    /// Default minimum margin percentage for this client (optional, only for Admin/CFO)
    /// </summary>
    public decimal? DefaultMinimumMarginPercent { get; set; }

    /// <summary>
    /// Discount percentage applied to this client (optional, only for Admin/CFO)
    /// Used in margin calculations
    /// </summary>
    public decimal? DiscountPercent { get; set; }

    /// <summary>
    /// Number of forced/imposed vacation days per year for this client (optional, only for Admin/CFO)
    /// Used in margin calculations
    /// </summary>
    public int? ForcedVacationDaysPerYear { get; set; }

    /// <summary>
    /// Target hourly rate (selling rate target in $/h) for this client (optional, only for Admin/CFO)
    /// Used in margin calculations
    /// </summary>
    public decimal? TargetHourlyRate { get; set; }

    /// <summary>
    /// Computed property: indicates if all financial parameters required by CFO are configured
    /// </summary>
    public bool IsFinancialConfigComplete =>
        DefaultTargetMarginPercent.HasValue
        && DefaultMinimumMarginPercent.HasValue
        && DiscountPercent.HasValue
        && ForcedVacationDaysPerYear.HasValue
        && TargetHourlyRate.HasValue;

    /// <summary>
    /// Name of primary contact person
    /// </summary>
    public string ContactName { get; set; } = string.Empty;

    /// <summary>
    /// Email of primary contact person
    /// </summary>
    public string ContactEmail { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if this client is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual Sector Sector { get; set; } = null!;
    public virtual BusinessUnit BusinessUnit { get; set; } = null!;
    public virtual Country Country { get; set; } = null!;
    public virtual Currency Currency { get; set; } = null!;
}
