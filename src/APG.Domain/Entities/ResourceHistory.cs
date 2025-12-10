namespace APG.Domain.Entities;

/// <summary>
/// Tracks all changes made to a Resource over time
/// This allows tracking salary increases, rate changes, and their impact on project margins
/// </summary>
public class ResourceHistory
{
    public int Id { get; set; }
    
    // Link to the Resource
    public int ResourceId { get; set; }
    public Resource Resource { get; set; } = null!;
    
    // What changed
    public string ChangeType { get; set; } = string.Empty; // "RateUpdate", "SalaryIncrease", "ProfileUpdate", etc.
    public string ChangeDescription { get; set; } = string.Empty; // Human-readable description
    
    // Old values (JSON for flexibility)
    public string? OldValuesJson { get; set; }
    
    // New values (JSON for flexibility)
    public string? NewValuesJson { get; set; }
    
    // Specific tracked fields for easy querying
    public decimal? OldDailyCostRate { get; set; }
    public decimal? NewDailyCostRate { get; set; }
    public decimal? OldDailySellRate { get; set; }
    public decimal? NewDailySellRate { get; set; }
    public decimal? OldMarginRate { get; set; }
    public decimal? NewMarginRate { get; set; }
    
    // Impact analysis
    public string? ImpactNotes { get; set; } // E.g., "Impacted 3 active projects"
    
    // Audit fields
    public string? ChangedBy { get; set; } // User who made the change
    public DateTime ChangedAt { get; set; }
    public bool IsActive { get; set; } = true;
}
