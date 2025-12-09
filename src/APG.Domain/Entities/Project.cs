namespace APG.Domain.Entities;

/// <summary>
/// Project entity representing projects in the system
/// </summary>
public class Project : BaseEntity
{
    /// <summary>
    /// Project name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Project code - entered by user (e.g., "PRJ-2024-001")
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Foreign key to Client
    /// </summary>
    public int ClientId { get; set; }

    /// <summary>
    /// Navigation property to Client
    /// </summary>
    public Client Client { get; set; } = null!;

    /// <summary>
    /// Foreign key to Business Unit
    /// </summary>
    public int BusinessUnitId { get; set; }

    /// <summary>
    /// Navigation property to Business Unit
    /// </summary>
    public BusinessUnit BusinessUnit { get; set; } = null!;

    /// <summary>
    /// Project type: T&M, Forfait, Autre
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Project manager/responsible name
    /// </summary>
    public string? ResponsibleName { get; set; }

    /// <summary>
    /// Currency for the project (CAD, USD, EUR)
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Project start date
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Project end date
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Target margin percentage
    /// </summary>
    public decimal TargetMargin { get; set; }

    /// <summary>
    /// Minimum margin percentage
    /// </summary>
    public decimal MinMargin { get; set; }

    /// <summary>
    /// Project status: En construction, Actif, Terminé, En pause
    /// </summary>
    public string Status { get; set; } = "En construction";

    /// <summary>
    /// Additional notes about the project
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Year-to-date revenue (optional)
    /// </summary>
    public decimal? YtdRevenue { get; set; }

    /// <summary>
    /// Team members as JSON (for flexibility)
    /// </summary>
    public string? TeamMembersJson { get; set; }

    /// <summary>
    /// Global margin history as JSON
    /// </summary>
    public string? GlobalMarginHistoryJson { get; set; }

    /// <summary>
    /// Indicates if this project is active
    /// </summary>
    public bool IsActive { get; set; } = true;
}
