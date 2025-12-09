namespace APG.Domain.Entities;

/// <summary>
/// Global salary settings entity for calculator configuration
/// Used to compute internal cost for salaried resources
/// Only one active record should exist at any time
/// </summary>
public class GlobalSalarySettings : BaseEntity
{
    /// <summary>
    /// Employer charges rate as a percentage (e.g., 65.00 for 65%)
    /// </summary>
    public decimal EmployerChargesRate { get; set; }

    /// <summary>
    /// Indirect annual costs (currency amount, e.g., 5000.00)
    /// </summary>
    public decimal IndirectAnnualCosts { get; set; }

    /// <summary>
    /// Billable hours per year (e.g., 1600)
    /// </summary>
    public int BillableHoursPerYear { get; set; }

    /// <summary>
    /// Indicates if this is the currently active configuration
    /// Only one record should be active at any time
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Indicates if this record has been soft deleted
    /// Soft deleted records are excluded from queries by global query filter
    /// </summary>
    public bool IsDeleted { get; set; } = false;
}
