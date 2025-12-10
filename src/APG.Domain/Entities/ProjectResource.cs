namespace APG.Domain.Entities;

/// <summary>
/// ProjectResource entity - Many-to-Many relationship between Projects and Resources
/// Represents a resource assignment to a specific project with assignment details
/// </summary>
public class ProjectResource : BaseEntity
{
    /// <summary>
    /// Foreign key to Project
    /// </summary>
    public int ProjectId { get; set; }

    /// <summary>
    /// Navigation property to Project
    /// </summary>
    public Project Project { get; set; } = null!;

    /// <summary>
    /// Foreign key to Resource
    /// </summary>
    public int ResourceId { get; set; }

    /// <summary>
    /// Navigation property to Resource
    /// </summary>
    public Resource Resource { get; set; } = null!;

    /// <summary>
    /// Role of the resource in this project (e.g., Développeur, Architecte, QA, etc.)
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Resource type for this assignment: Employé or Pigiste
    /// </summary>
    public string ResourceType { get; set; } = "Employé";

    /// <summary>
    /// Assignment start date
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Assignment end date (nullable for ongoing assignments)
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Daily cost rate for this specific assignment (can differ from resource's default)
    /// </summary>
    public decimal DailyCostRate { get; set; }

    /// <summary>
    /// Daily sell rate for this specific assignment (can differ from resource's default)
    /// </summary>
    public decimal DailySellRate { get; set; }

    /// <summary>
    /// Gross margin amount per day
    /// </summary>
    public decimal GrossMarginAmount { get; set; }

    /// <summary>
    /// Gross margin percentage
    /// </summary>
    public decimal GrossMarginPercent { get; set; }

    /// <summary>
    /// Net margin percentage (after indirect costs)
    /// </summary>
    public decimal NetMarginPercent { get; set; }

    /// <summary>
    /// Number of billable days worked (optional, for reporting)
    /// </summary>
    public int? BilledDays { get; set; }

    /// <summary>
    /// Assignment status: Active, Completed, Cancelled
    /// </summary>
    public string Status { get; set; } = "Active";

    /// <summary>
    /// Additional notes about this assignment
    /// </summary>
    public string? Notes { get; set; }
}
