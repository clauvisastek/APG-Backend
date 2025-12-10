namespace APG.Domain.Entities;

/// <summary>
/// Resource entity representing human resources (employees and freelancers) in the system
/// </summary>
public class Resource : BaseEntity
{
    /// <summary>
    /// Full name of the resource (computed or stored)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// First name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Email - UNIQUE identifier for resource
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Foreign key to Business Unit
    /// </summary>
    public int BusinessUnitId { get; set; }

    /// <summary>
    /// Navigation property to Business Unit
    /// </summary>
    public BusinessUnit BusinessUnit { get; set; } = null!;

    /// <summary>
    /// Job type: Développeur, QA, Analyste d'affaires, Architecte, Autres
    /// </summary>
    public string JobType { get; set; } = string.Empty;

    /// <summary>
    /// Seniority level: Junior, Intermédiaire, Sénior, Expert
    /// </summary>
    public string Seniority { get; set; } = string.Empty;

    /// <summary>
    /// Current client name (optional)
    /// </summary>
    public string? CurrentClient { get; set; }

    /// <summary>
    /// Current mission/project name (optional)
    /// </summary>
    public string? CurrentMission { get; set; }

    /// <summary>
    /// Resource status: Actif en mission, Disponible, En intercontrat
    /// </summary>
    public string Status { get; set; } = "Disponible";

    /// <summary>
    /// Daily cost rate (taux coûtant journalier)
    /// </summary>
    public decimal DailyCostRate { get; set; }

    /// <summary>
    /// Daily sell rate (taux vendant journalier)
    /// </summary>
    public decimal DailySellRate { get; set; }

    /// <summary>
    /// Calculated margin rate percentage
    /// </summary>
    public decimal MarginRate { get; set; }

    /// <summary>
    /// Hire date at the company
    /// </summary>
    public DateTime HireDate { get; set; }

    /// <summary>
    /// Manager name (optional)
    /// </summary>
    public string? Manager { get; set; }

    /// <summary>
    /// Phone number (optional)
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Navigation property - Projects this resource is assigned to
    /// </summary>
    public ICollection<ProjectResource> ProjectAssignments { get; set; } = new List<ProjectResource>();
}
