namespace APG.Domain.Entities;

/// <summary>
/// Client margin settings entity for per-client calculator configuration
/// Stores target margin percentage and target hourly rate per client
/// </summary>
public class ClientMarginSettings : BaseEntity
{
    /// <summary>
    /// Foreign key to Client
    /// </summary>
    public int ClientId { get; set; }

    /// <summary>
    /// Target margin percentage (e.g., 25.00 for 25%)
    /// </summary>
    public decimal TargetMarginPercent { get; set; }

    /// <summary>
    /// Target hourly rate (e.g., 150.00 $/h)
    /// </summary>
    public decimal TargetHourlyRate { get; set; }

    /// <summary>
    /// Navigation property to Client
    /// </summary>
    public Client Client { get; set; } = null!;
}
