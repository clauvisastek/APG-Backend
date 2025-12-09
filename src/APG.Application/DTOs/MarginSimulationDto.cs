namespace APG.Application.DTOs;

public class MarginSimulationRequest
{
    public string ResourceType { get; set; } = string.Empty; // "Salarie" | "Pigiste"
    public decimal? AnnualGrossSalary { get; set; }
    public int ClientId { get; set; }
    public decimal ProposedBillRate { get; set; }
    public int? PlannedHours { get; set; }
    public string? Seniority { get; set; }
}

public class MarginSimulationResponse
{
    public TargetResults TargetResults { get; set; } = new();
    public ProposedResults ProposedResults { get; set; } = new();
}

public class TargetResults
{
    public decimal CostPerHour { get; set; }
    public decimal EffectiveTargetBillRate { get; set; }
    public decimal TheoreticalMarginPercent { get; set; }
    public decimal TheoreticalMarginPerHour { get; set; }
    public decimal ConfiguredTargetMarginPercent { get; set; }
    public decimal ConfiguredMinMarginPercent { get; set; }
    public decimal? ConfiguredDiscountPercent { get; set; }
    public int ForcedVacationDaysPerYear { get; set; }
    public string Status { get; set; } = string.Empty; // "OK" | "WARNING" | "KO"
}

public class ProposedResults
{
    public decimal ProposedBillRate { get; set; }
    public decimal MarginPercent { get; set; }
    public decimal MarginPerHour { get; set; }
    public decimal? DiscountPercentApplied { get; set; }
    public string Status { get; set; } = string.Empty; // "OK" | "WARNING" | "KO"
}
